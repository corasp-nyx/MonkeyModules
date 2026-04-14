using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace corasp_nyx.MonkeyModules.Presets
{
    public class AddModifier : Modifier<float> // (using 'float?' as generic type might be necessary?) (it doesnt seem to be)
    {
        public override int priority { get; protected set; }

        private const string calculationEventKeySuffix = "-Calculate";

        protected Attribute<float> addend;

        protected bool subtract;

        /// <param name="subtract">Subtracts addend instead of adding if set to true.</param>
        public AddModifier(IEnumerable<ModifierRequirement> requirements, Attribute<float> addend, bool subtract = false, int priority = (int)ModifierPriority.mainAdd) : base(requirements)
        {
            this.priority = priority;
            this.addend = addend;
            this.subtract = subtract;
            // decommission if addend attribute is decommissioned
            addend.OnChange.AddListener((changedAttributes) => { if (changedAttributes.LastOrDefault()?.decommissioned ?? false) Decommission(); else NotifyOfVariableChange(changedAttributes); }, uid + calculationEventKeySuffix);
        }

        public override void Modify(ref float value, Attribute attribute)
        {
            if (!addend.decommissioned)
                value = subtract ? value - addend.GetValue() : value + addend.GetValue();
        }

        /// <returns>Whether Modifier addends match.</returns>
        protected override bool MatchesCalculationOf(Modifier<float> other)
        {
            return addend == (other as AddModifier)?.addend;
        }
    }

    public class AddConstantModifier : Modifier<float>
    {
        public override int priority { get; protected set; }

        protected float addend;

        public AddConstantModifier(IEnumerable<ModifierRequirement> requirements, float addend, int priority = (int)ModifierPriority.mainAdd) : base(requirements)
        {
            this.priority = priority;
            this.addend = addend;
        }
        
        public override void Modify(ref float value, Attribute attribute)
        {
            value = value + addend;
        }

        /// <returns>Whether Modifier addends match.</returns>
        protected override bool MatchesCalculationOf(Modifier<float> other)
        {
            return addend == (other as AddConstantModifier)?.addend;
        }
    }
}
