using System;
using System.Collections.Generic;

#nullable enable
namespace corasp_nyx.MonkeyModules.Presets
{
    public class ChangedValueEventModifier<T> : EventModifier<T>
    {
        public ChangedValueEventModifier(IEnumerable<ModifierRequirement> requirements) : base(requirements) { }

        protected override bool CheckTrigger(T? oldValue, T? newValue, Attribute<T> attribute)
        {
            return !(oldValue?.Equals(newValue) ?? newValue == null);
        }

        protected override bool MatchesCalculationOf(Modifier<T> other) => other is ChangedValueEventModifier<T>;
    }

    public class TargetValueEventModifier<T> : EventModifier<T>
    {
        protected readonly T targetValue;

        public TargetValueEventModifier(IEnumerable<ModifierRequirement> requirements, T targetValue) : base(requirements)
        {
            this.targetValue = targetValue;
        }

        protected override bool CheckTrigger(T? oldValue, T? newValue, Attribute<T> attribute)
        {
            return !(oldValue?.Equals(newValue) ?? newValue == null) && (newValue?.Equals(targetValue) ?? targetValue == null);
        }

        protected override bool MatchesCalculationOf(Modifier<T> other)
        {
            return other is TargetValueEventModifier<T> ? (other as TargetValueEventModifier<T>)?.targetValue?.Equals(targetValue) ?? targetValue == null : false;
        }
    }

    public class IncreasedValueEventModifier : EventModifier<float>
    {
        public IncreasedValueEventModifier(IEnumerable<ModifierRequirement> requirements) : base(requirements) { }

        protected override bool CheckTrigger(float oldValue, float newValue, Attribute<float> attribute)
        {
            return newValue > oldValue;
        }

        protected override bool MatchesCalculationOf(Modifier<float> other) => true;
    }

    public class DecreasedValueEventModifier : EventModifier<float>
    {
        public DecreasedValueEventModifier(IEnumerable<ModifierRequirement> requirements) : base(requirements) { }

        protected override bool CheckTrigger(float oldValue, float newValue, Attribute<float> attribute)
        {
            return newValue < oldValue;
        }

        protected override bool MatchesCalculationOf(Modifier<float> other) => true;
    }

    public class CustomEventModifier<T> : EventModifier<T>
    {
        protected Func<T?, T?, Attribute<T>, bool> trigger;

        /// <param name="trigger">Calculation result trigger. Target old value, new value, and Attribute in, trigger out.</param>
        public CustomEventModifier(IEnumerable<ModifierRequirement> requirements, Func<T?, T?, Attribute<T>, bool> trigger) : base(requirements)
        {
            this.trigger = trigger;
        }

        protected override bool CheckTrigger(T? oldValue, T? newValue, Attribute<T> attribute)
        {
            return trigger(oldValue, newValue, attribute);
        }

        protected override bool MatchesCalculationOf(Modifier<T> other) => (other as CustomEventModifier<T>)?.trigger.Equals(trigger) ?? false;
    }
}
