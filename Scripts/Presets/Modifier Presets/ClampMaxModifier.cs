using System;
using System.Collections.Generic;

#nullable enable
namespace TDP.InteractiveComponents.Presets
{
    public class ClampMaxModifier : Modifier<float>
    {
        public override int priority { get; protected set; }

        private const string calculationEventKeySuffix = "-Calculate";

        protected Attribute<float> max;

        public ClampMaxModifier(IEnumerable<ModifierRequirement> requirements, Attribute<float> max, int priority = (int)ModifierPriority.clamp) : base(requirements)
        {
            this.priority = priority;
            this.max = max;
            // decommission if max attribute is decommissioned
            max.OnChange.AddListener((changedAttributes) => { if (changedAttributes.FindLastIndex(attribute => attribute.decommissioned) == changedAttributes.Count - 1) Decommission(); else NotifyOfVariableChange(changedAttributes); }, uid + calculationEventKeySuffix);
        }

        public override void Modify(ref float value, Attribute attribute)
        {
            if (!max.decommissioned)
                value = Math.Min(value, max.GetValue());
        }

        /// <returns>Whether Modifier minimum values match.</returns>
        protected override bool MatchesCalculationOf(Modifier<float> other)
        {
            return max == (other as ClampMaxModifier)?.max;
        }
    }

    public class ClampMaxConstantModifier : Modifier<float>
    {
        public override int priority { get; protected set; }

        protected float max;

        public ClampMaxConstantModifier(IEnumerable<ModifierRequirement> requirements, float max, int priority = (int)ModifierPriority.clamp) : base(requirements)
        {
            this.priority = priority;
            this.max = max;
        }

        public override void Modify(ref float value, Attribute attribute)
        {
            value = Math.Min(value, max);
        }

        /// <returns>Whether Modifier minimum values match.</returns>
        protected override bool MatchesCalculationOf(Modifier<float> other)
        {
            return max == (other as ClampMaxConstantModifier)?.max;
        }
    }
}
