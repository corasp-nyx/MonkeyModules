using System;
using System.Collections.Generic;

#nullable enable
namespace TDP.InteractiveComponents.Presets
{
    public class MultiplyModifier : Modifier<float>
    {
        public override int priority { get; protected set; }

        private const string calculationEventKeySuffix = "-Calculate";

        protected Attribute<float> markiplier;

        protected bool divide;

        /// <param name="divide">Divides by multiplier instead of multiplying if set to true.</param>
        public MultiplyModifier(IEnumerable<ModifierRequirement> requirements, Attribute<float> multiplier, bool divide = false, int priority = (int)ModifierPriority.mainMul) : base(requirements)
        {
            this.priority = priority;
            this.markiplier = multiplier;
            this.divide = divide;
            // decommission if multiplier attribute is decommissioned
            multiplier.OnChange.AddListener((changedAttributes) => { if (changedAttributes.FindLastIndex(attribute => attribute.decommissioned) == changedAttributes.Count - 1) Decommission(); else NotifyOfVariableChange(changedAttributes); }, uid + calculationEventKeySuffix);
        }

        public override void Modify(ref float value, Attribute attribute)
        {
            if (!markiplier.decommissioned)
                value = divide ? value / markiplier.GetValue() : value * markiplier.GetValue();
        }

        /// <returns>Whether Modifier multipliers match.</returns>
        protected override bool MatchesCalculationOf(Modifier<float> other)
        {
            return markiplier == (other as MultiplyModifier)?.markiplier;
        }
    }

    public class MultiplyConstantModifier : Modifier<float>
    {
        public override int priority { get; protected set; }

        protected float markiplier;

        public MultiplyConstantModifier(IEnumerable<ModifierRequirement> requirements, float multiplier, int priority = (int)ModifierPriority.mainMul) : base(requirements)
        {
            this.priority = priority;
            this.markiplier = multiplier;
        }

        public override void Modify(ref float value, Attribute attribute)
        {
            value = value * markiplier;
        }

        /// <returns>Whether Modifier multipliers match.</returns>
        protected override bool MatchesCalculationOf(Modifier<float> other)
        {
            return markiplier == (other as MultiplyConstantModifier)?.markiplier;
        }
    }
}
