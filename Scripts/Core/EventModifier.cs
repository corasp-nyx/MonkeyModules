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
                Attribute<T> target = (Attribute<T>)attribute;
                target.OnChange.RemoveListener(uid + triggerEventKeySuffix);
                T? cachedValue = target.GetValue();
                target.OnChange.AddListener((_) => { target.OnChange.RemoveListener(uid + triggerEventKeySuffix); if (AppliesTo(target) && CheckTrigger(cachedValue, target.GetValue(), target)) OnTriggered.Invoke(target); }, uid + triggerEventKeySuffix);
            }
        }

        protected sealed override void NotifyOfVariableChange(List<Attribute> changedAttributes) => base.NotifyOfVariableChange(changedAttributes);

        /// <summary>
        /// Check if the change of an affected Attribute's value matches Event trigger conditions.
        /// </summary>
        protected abstract bool CheckTrigger(T? oldValue, T? newValue, Attribute<T> attribute);
    }
}
