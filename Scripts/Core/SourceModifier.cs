using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace TDP.InteractiveComponents
{
    /// <summary>
    /// A Modifier that replaces the value of modified Attributes with the value of another source Attribute matching specified conditions.
    /// </summary>
    /// <typeparam name="T">Affected and sourced Attribute value Type.</typeparam>
    public class SourceModifier<T> : Modifier<T>
    {
        public override int priority { get; protected set; }

        private const string calculationEventKeySuffix = "-Calculate";
        private const string anySourceChangedEventKeySuffix = "-AnyChanged";

        /// <summary>This Event is called whenever the value of any available applicable source Attribute changes. (not just the only one selected for modification)</summary>
        public readonly Event<List<Attribute>> OnAnySourceChanged = new Event<List<Attribute>>();

        public enum SourceTarget
        {
            newest = -1, oldest = 0, index1 = 1, index2 = 2
        }

        protected readonly IEnumerable<ModifierRequirement> sourceRequirements;
        protected readonly int target;
        protected readonly Func<Attribute<T>, object>? targetSelector;
        protected List<Attribute<T>>? availableSources;
        protected Attribute<T>? targetSource;

        /// <param name="sourceRequirements">The requirements for Attributes forming the source pool.</param>
        /// <param name="targetIndex">SourceModifier&lt;T&gt;.SourceTarget can be used to select which available source should be targeted, or simply an index, of which the closest element will be chosen.</param>
        public SourceModifier(IEnumerable<ModifierRequirement> requirements, IEnumerable<ModifierRequirement> sourceRequirements, int targetIndex = (int)SourceTarget.oldest, int priority = (int)ModifierPriority.baseValue) : base(requirements)
        {
            this.priority = priority;
            this.sourceRequirements = sourceRequirements;
            this.target = targetIndex;
        }

        /// <param name="sourceRequirements">The requirements for Attributes forming the source pool.</param>
        /// <param name="targetSelector">The selector used to order available source Attributes, of which the first in a descending order gets chosen.</param>
        public SourceModifier(IEnumerable<ModifierRequirement> requirements, IEnumerable<ModifierRequirement> sourceRequirements, Func<Attribute<T>, object> targetSelector, int priority = (int)ModifierPriority.baseValue) : base(requirements)
        {
            this.priority = priority;
            this.sourceRequirements = sourceRequirements;
            this.target = -2;
            this.targetSelector = targetSelector;
        }

        public override void Modify(ref T? value, Attribute attribute)
        {
            if (targetSource != null && !targetSource.decommissioned)
                value = targetSource.GetValue();
        }

        /// <returns>All available source Attributes matching requirements.</returns>
        public virtual Attribute<T>[] GetAllSources()
        {
            return availableSources?.ToArray() ?? new Attribute<T>[0];
        }

        public override bool AppliesTo(Attribute attribute) // hijack method usually called by all existing attributes when the modifier gets newly created, and once by each newly created attribute afterwards, to catch applicable sources
        {
            if (attribute is Attribute<T> && (sourceRequirements?.All(requirement => requirement.IsApplicable(attribute)) ?? true))
            {
                // add to list of available sources
                availableSources ??= new List<Attribute<T>>();
                if (!availableSources.Contains(attribute))
                {
                    availableSources.Add((Attribute<T>)attribute);
                    attribute.OnChange.AddListener(OnAnySourceChanged.Invoke, uid + anySourceChangedEventKeySuffix);
                }

                // refresh targeted source
                NotifyOfVariableChange(new List<Attribute>());
            }

            return base.AppliesTo(attribute);
        }

        protected override void NotifyOfVariableChange(List<Attribute> changedAttributes)
        {
            // remove decommissioned addends from list of available ones
            if (availableSources != null)
                foreach (Attribute<T> source in new List<Attribute<T>>(availableSources))
                    if (source.decommissioned)
                    {
                        source.OnChange.RemoveListener(uid + anySourceChangedEventKeySuffix);
                        availableSources.Remove(source);
                    }

            // if current source is no longer correct target
            if (targetSource != SelectTargetSource())
            {
                // unregister from old source attribute change event
                targetSource?.OnChange.RemoveListener(uid + calculationEventKeySuffix);

                // set new source
                targetSource = SelectTargetSource();
                targetSource?.OnChange.AddListener(NotifyOfVariableChange, uid + calculationEventKeySuffix);
            }

            base.NotifyOfVariableChange(changedAttributes);
        }

        /// <returns>Desired Attribute selected from available sources.</returns>
        protected virtual Attribute<T>? SelectTargetSource()
        {
            if (availableSources != null && availableSources.Count > 0)
                if (targetSelector != null)
                    return availableSources.OrderByDescending(targetSelector).First();
                else
                    switch (target)
                        {
                            case (int)SourceTarget.newest:
                                return availableSources.Last();
                            case (int)SourceTarget.oldest:
                                return availableSources.First();
                            default:
                                return target < availableSources.Count - 1 ? (target >= (int)SourceTarget.newest ? availableSources[target] : availableSources.First()) : availableSources.Last();
                        }
            else
                return null;
        }

        /// <returns>Whether Modifier source requirements match.</returns>
        protected override bool MatchesCalculationOf(Modifier<T> other)
        {
            return sourceRequirements.SequenceEqual(((SourceModifier<T>)other).sourceRequirements);
        }
    }
}
