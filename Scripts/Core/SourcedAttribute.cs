using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace TDP.InteractiveComponents
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
            sourceModifier = GlobalManager.AddModifier(new SourceModifier<T>(new List<ModifierRequirement>() { new ModifierAttributeIdRequirement(uid) }, sourceRequirements, targetIndex));
        }

        public override void Decommission()
        {
            // decommission personal source modifier along with this attribute
            sourceModifier.Decommission();
        }
    }

    /// <summary>
    /// An Attribute sourcing its base value from the combined values of any other Attributes matching specified conditions.
    /// </summary>
    public class CombinedSourcedAttribute : SourcedAttribute<float>
    {
        public enum CombinationProcess
        {
            add, multiply
        }

        protected Func<float, float, float> combination;

        /// <param name="baseValue">Fallback value used when no source is available.</param>
        /// <param name="sourceRequirements">The requirements for Attributes forming the source pool.</param>
        /// <param name="combination">The combination method used on the source values.</param>
        public CombinedSourcedAttribute(string name, float baseValue, IEnumerable<ModifierRequirement> sourceRequirements, CombinationProcess combination) : base(name, baseValue, sourceRequirements, -2)
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

            AdjustedSourceModifierSetup();
        }

        /// <param name="baseValue">Fallback value used when no source is available.</param>
        /// <param name="sourceRequirements">The requirements for Attributes forming the source pool.</param>
        /// <param name="combinationAggregate">The aggregate used to combine the source values.</param>
        public CombinedSourcedAttribute(string name, float baseValue, IEnumerable<ModifierRequirement> sourceRequirements, Func<float, float, float> combinationAggregate) : base(name, baseValue, sourceRequirements, -2)
        {
            combination = combinationAggregate;

            AdjustedSourceModifierSetup();
        }

        private void AdjustedSourceModifierSetup()
        {
            // listen to changes from all sources used in combination
            sourceModifier.OnAnySourceChanged.AddListener(NotifyOfModifierChange, uid + calculationEventKeySuffix);

            // reduce source modifier functions to manual output (todo: check if a duplicate returned by ecsmanager could ruin this) (also decommission in parent) (relevant question: do ModifierRequirements equal each other?)
            GlobalManager.RemoveModifier(sourceModifier);
            appliedModifiers?.Remove(sourceModifier);
            sourceModifier.OnChange.RemoveListener(uid + calculationEventKeySuffix);
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
}
