/*
 *  Monkey Modules
 *  Copyright (c) 2026 corasp~nyx
 *
 *  Licensed under the MIT License; you may only use this file in compliance with it.
 */

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace corasp_nyx.MonkeyModules
{
    /// <summary>
    /// An Attribute sourcing its base value from the value of another Attribute matching specified conditions.
    /// </summary>
    /// <typeparam name="T">Sourced Attribute value Type.</typeparam>
    public class SourcedAttribute<T> : Attribute<T>
    {
        protected SourceModifier<T> sourceModifier; // parasite

        /// <param name="baseValue">Fallback value used when no source is available.</param>
        /// <param name="sourceRequirements">The requirements for Attributes forming the source pool.</param>
        /// <param name="targetIndex">SourceModifier&lt;T&gt;.SourceTarget can be used to select which available source should be targeted, or simply an index, of which the closest element will be chosen.</param>
        public SourcedAttribute(string name, T? baseValue, IEnumerable<ModifierRequirement> sourceRequirements, int targetIndex = (int)SourceModifier<T>.SourceTarget.oldest) : base(name, baseValue)
        {
            // create personal source modifier and publish it to gather sources
            appliedModifiers ??= new List<Modifier>();
            sourceModifier = new SourceModifier<T>(new List<ModifierRequirement>() { new ModifierAttributeIdRequirement(uid) }, sourceRequirements, targetIndex);
            GlobalManager.AddModifier(sourceModifier); // (added seperately to prevent issues in inheriting attributes calculating before initialisation)
            OnDecommission.AddListener(sourceModifier.Decommission, sourceModifier.uid + "-Discarding");
        }
    }

    /// <summary>
    /// An Attribute sourcing its base value from the combined values of any other Attributes matching specified conditions.
    /// </summary>
    public class SourcedCombinedAttribute : SourcedAttribute<float>
    {
        public enum CombinationProcess
        {
            add, multiply
        }

        protected Func<float, float, float> combination;

        /// <param name="baseValue">Fallback value used when no source is available.</param>
        /// <param name="sourceRequirements">The requirements for Attributes forming the source pool.</param>
        /// <param name="combination">The combination method used on the source values.</param>
        public SourcedCombinedAttribute(string name, float baseValue, IEnumerable<ModifierRequirement> sourceRequirements, CombinationProcess combination) : base(name, baseValue, sourceRequirements, -2)
        {
            switch (combination)
            {
                case CombinationProcess.add:
                    this.combination = (sum, next) => sum + next;
                    break;
                case CombinationProcess.multiply:
                    this.combination = (product, next) => product * next;
                    break;
                default:
                    this.combination = (total, next) => baseValue;
                    break;
            }

            // listen to changes from all sources used in combination
            sourceModifier.OnAnySourceChanged.AddListener(NotifyOfModifierChange, uid + calculationEventKeySuffix);
        }

        /// <param name="baseValue">Fallback value used when no source is available.</param>
        /// <param name="sourceRequirements">The requirements for Attributes forming the source pool.</param>
        /// <param name="combinationAggregate">The aggregate used to combine the source values.</param>
        public SourcedCombinedAttribute(string name, float baseValue, IEnumerable<ModifierRequirement> sourceRequirements, Func<float, float, float> combinationAggregate) : base(name, baseValue, sourceRequirements, (int)SourceModifier<bool>.SourceTarget.none)
        {
            combination = combinationAggregate;

            // listen to changes from all sources used in combination
            sourceModifier.OnAnySourceChanged.AddListener(NotifyOfModifierChange, uid + calculationEventKeySuffix);
        }

        protected override bool Calculate() // inject combined source values into calculation as base value
        {
            // cache base value for restoration
            float cachedBaseValue = baseValue;

            // combine source values
            baseValue = sourceModifier.GetAllSources().Select(attribute => attribute.GetValue()).Aggregate(combination);

            // calculate and restore base value
            bool hasChanged = base.Calculate();
            baseValue = cachedBaseValue;
            return hasChanged;
        }
    }

    /// <summary>
    /// An Attribute which flips its base value whenever an other Attribute matching specified conditions has a value contradicting the default base value.
    /// </summary>
    public class SourcedSwitchAttribute : SourcedAttribute<bool>
    {
        protected readonly object calculationLock = new();

        /// <param name="defaultValue">Default unmodified value.</param>
        /// <param name="sourceRequirements">The requirements for Attributes forming the source pool.</param>
        public SourcedSwitchAttribute(string name, bool defaultValue, IEnumerable<ModifierRequirement> sourceRequirements) : base(name, defaultValue, sourceRequirements, (int)SourceModifier<bool>.SourceTarget.none)
        {
            // listen to changes from all sources used in combination
            sourceModifier.OnAnySourceChanged.AddListener(NotifyOfModifierChange, uid + calculationEventKeySuffix);
        }

        protected override bool Calculate() // inject combined source values into calculation as base value
        {
            lock (calculationLock) // (might add this everywhere)
            {
                // cache base value for restoration
                bool cachedBaseValue = baseValue;

                // find modifying source value
                if (sourceModifier.GetAllSources().Select(attribute => attribute.GetValue()).Any(sourceValue => sourceValue == !baseValue)) // (doesnt seem to work for some reason. todo: fix)
                    baseValue = !baseValue;

                // calculate and restore base value
                bool hasChanged = base.Calculate();
                baseValue = cachedBaseValue;
                return hasChanged;
            }
        }
    }
}
