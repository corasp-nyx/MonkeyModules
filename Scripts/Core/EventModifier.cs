/*
 *  Monkey Modules
 *  Copyright (c) 2026 corasp~nyx
 *
 *  Licensed under the MIT License; you may only use this file in compliance with it.
 */

using System;
using System.Collections.Generic;

#nullable enable
namespace corasp_nyx.MonkeyModules
{
    /// <summary>
    /// A Modifier that invokes an Event when triggered by the value of an affected Attribute changing in a specified way. Does not apply any value modifications.
    /// </summary>
    /// <typeparam name="T">Affected Attribute value Type.</typeparam>
    public abstract class EventModifier<T> : Modifier<T>
    {
        public sealed override int priority { get; protected set; } = int.MinValue;

        /// <summary>Event called when the change of an affected Attribute's value matches triggering conditions.</summary>
        public readonly Event<Attribute<T>> OnTriggered = new Event<Attribute<T>>();

        protected const string triggerEventKeySuffix = "-Trigger";

        public EventModifier(IEnumerable<ModifierRequirement> requirements) : base(requirements) { }

        public sealed override void Modify(ref T? value, Attribute attribute)
        {
            if (attribute is Attribute<T>)
            {
                Attribute<T> trigger = (Attribute<T>)attribute;
                trigger.OnChange.RemoveListener(uid + triggerEventKeySuffix);
                T? cachedValue = trigger.GetValue();
                trigger.OnChange.AddListener((_) => { trigger.OnChange.RemoveListener(uid + triggerEventKeySuffix); if (CheckTrigger(cachedValue, trigger.GetValue(), trigger)) OnTriggered.Invoke(trigger); }, uid + triggerEventKeySuffix);
            }
        }

        protected sealed override void NotifyOfVariableChange(List<Attribute> changedAttributes) => base.NotifyOfVariableChange(changedAttributes);

        /// <summary>
        /// Check if the change of an affected Attribute's value matches Event trigger conditions.
        /// </summary>
        protected abstract bool CheckTrigger(T? oldValue, T? newValue, Attribute<T> attribute);
    }

    public class ChangedValueEventModifier<T> : EventModifier<T>
    {
        public ChangedValueEventModifier(IEnumerable<ModifierRequirement> requirements) : base(requirements) { }

        protected override bool CheckTrigger(T? oldValue, T? newValue, Attribute<T> attribute)
        {
            return !oldValue?.Equals(newValue) ?? newValue == null;
        }

        protected override bool MatchesCalculationOf(Modifier<T> other) => true;
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
}
