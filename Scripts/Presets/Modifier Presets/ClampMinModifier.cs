using System;
using System.Collections.Generic;

#nullable enable
namespace corasp_nyx.MonkeyModules.Presets
{
    public class ClampMinModifier : Modifier<float>
    {
        public override int priority { get; protected set; }

        private const string calculationEventKeySuffix = "-Calculate";

        protected Attribute<float> min;

        public ClampMinModifier(IEnumerable<ModifierRequirement> requirements, Attribute<float> min, int priority = (int)ModifierPriority.clamp) : base(requirements)
        {
            this.priority = priority;
            this.min = min;
            // decommission if min attribute is decommissioned
            min.OnChange.AddListener((changedAttributes) => { if (changedAttributes.FindLastIndex(attribute => attribute.decommissioned) == changedAttributes.Count - 1) Decommission(); else NotifyOfVariableChange(changedAttributes); }, uid + calculationEventKeySuffix);
        }

        public override void Modify(ref float value, Attribute attribute)
        {
            if (!min.decommissioned)
                value = Math.Max(value, min.GetValue());
        }

        /// <returns>Whether Modifier maximum values match.</returns>
        protected override bool MatchesCalculationOf(Modifier<float> other)
        {
            return min == (other as ClampMinModifier)?.min;
        }
    }

    public class ClampMinConstantModifier : Modifier<float>
    {
        public override int priority { get; protected set; }

        protected float min;

        public ClampMinConstantModifier(IEnumerable<ModifierRequirement> requirements, float min, int priority = (int)ModifierPriority.clamp) : base(requirements)
        {
            this.priority = priority;
            this.min = min;
        }

        public override void Modify(ref float value, Attribute attribute)
        {
            value = Math.Max(value, min);
        }

        /// <returns>Whether Modifier maximum values match.</returns>
        protected override bool MatchesCalculationOf(Modifier<float> other)
        {
            return min == (other as ClampMinConstantModifier)?.min;
        }
    }
}
