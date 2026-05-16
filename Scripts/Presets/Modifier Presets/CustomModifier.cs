using System;
using System.Collections.Generic;

#nullable enable
namespace corasp_nyx.MonkeyModules.Presets
{
    public class CustomModifier<T> : Modifier<T>
    {
        public override int priority { get; protected set; }

        private const string calculationEventKeySuffix = "-Calculate";

        private Func<T?, Attribute, T?> modification;

        /// <param name="modification">Modification of target Attribute values. Target value and target Attribute in, modified value out.</param>
        /// <param name="variables">Attributes used in modification. Will pass on change and decommission events.</param>
        public CustomModifier(IEnumerable<ModifierRequirement> requirements, Func<T?, Attribute, T?> modification, Attribute[] variables, int priority) : base(requirements)
        {
            this.priority = priority;
            this.modification = modification;
            foreach (Attribute variable in variables)
            {
                variable.OnDecommission.AddListener(Decommission, decommissionEventKeySuffix); // (todo: apply this to older modifier presets)
                variable.OnChange.AddListener(NotifyOfVariableChange, calculationEventKeySuffix);
            }
        }

        public override void Modify(ref T? value, Attribute attribute)
        {
            if (!decommissioned) // (todo: apply this to older modifier presets)
                value = modification(value, attribute);
        }

        /// <returns>Whether modifications match.</returns>
        protected override bool MatchesCalculationOf(Modifier<T> other)
        {
            return (other as CustomModifier<T>)?.modification.Equals(modification) ?? false; // (not sure if this works)
        }
    }

    public class CustomConstantModifier<T> : Modifier<T>
    {
        public override int priority { get; protected set; }

        private Func<T?, Attribute, T?> modification;

        /// <param name="modification">Modification of target Attribute values. Target value and target Attribute in, modified value out.</param>
        public CustomConstantModifier(IEnumerable<ModifierRequirement> requirements, Func<T?, Attribute, T?> modification, int priority) : base(requirements)
        {
            this.priority = priority;
            this.modification = modification;
        }

        public override void Modify(ref T? value, Attribute attribute)
        {
            if (!decommissioned)
                value = modification(value, attribute);
        }

        /// <returns>Whether modifications match.</returns>
        protected override bool MatchesCalculationOf(Modifier<T> other)
        {
            return (other as CustomConstantModifier<T>)?.modification.Equals(modification) ?? false; // (not sure if this works)
        }
    }
}
