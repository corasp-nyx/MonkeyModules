using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace corasp_nyx.MonkeyModules.Presets
{
    /// <summary>
    /// A Modifier that replaces the value of modified Attributes with the value of a specified Attribute.
    /// </summary>
    public class OverwriteModifier<T> : Modifier<T>
    {
        public override int priority { get; protected set; }

        private const string calculationEventKeySuffix = "-Calculate";

        protected Attribute<T> source;

        public OverwriteModifier(IEnumerable<ModifierRequirement> requirements, Attribute<T> source, int priority = (int)ModifierPriority.postAdd) : base(requirements)
        {
            this.priority = priority;
            this.source = source;
            // decommission if source attribute is decommissioned
            source.OnChange.AddListener((changedAttributes) => { if (changedAttributes.LastOrDefault()?.decommissioned ?? false) Decommission(); else NotifyOfVariableChange(changedAttributes); }, uid + calculationEventKeySuffix);
        }

        public override void Modify(ref T? value, Attribute attribute)
        {
            if (!source.decommissioned)
                value = source.GetValue();
        }

        /// <returns>Whether Modifier sources match.</returns>
        protected override bool MatchesCalculationOf(Modifier<T> other)
        {
            return source == (other as OverwriteModifier<T>)?.source;
        }
    }

    /// <summary>
    /// A Modifier that replaces the value of modified Attributes with a specified value.
    /// </summary>
    public class OverwriteConstantModifier<T> : Modifier<T>
    {
        public override int priority { get; protected set; }

        protected T? value;

        public OverwriteConstantModifier(IEnumerable<ModifierRequirement> requirements, T? value, int priority = (int)ModifierPriority.postAdd) : base(requirements)
        {
            this.priority = priority;
            this.value = value;
        }

        public override void Modify(ref T? value, Attribute attribute)
        {
            value = this.value;
        }

        /// <returns>Whether Modifier values match.</returns>
        protected override bool MatchesCalculationOf(Modifier<T> other)
        {
            return other is OverwriteConstantModifier<T> ? value?.Equals(((OverwriteConstantModifier<T>)other).value) ?? ((OverwriteConstantModifier<T>)other).value == null : false;
        }
    }
}
