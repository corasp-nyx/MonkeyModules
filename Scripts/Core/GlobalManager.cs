/*
 *  Monkey Modules
 *  Copyright (c) 2026 corasp~nyx
 *
 *  Licensed under the MIT License; you may only use this file in compliance with it.
 */

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace corasp_nyx.MonkeyModules
{
    public static class GlobalManager
    {
        private static readonly Dictionary<string, Event<Module>> events;
        private static readonly List<Modifier> modifiers;
        //private static readonly List<Effect> effects;
        //private static readonly List<Attribute> attributes;

        public static readonly Event<Modifier> OnModifierPublished;

        static GlobalManager()
        {
            events ??= new Dictionary<string, Event<Module>>();
            modifiers ??= new List<Modifier>();
            //effects ??= new List<Effect>();
            //attributes ??= new List<Attribute>();

            OnModifierPublished = new Event<Modifier>();
        }

        /// <summary>
        /// Broadcasts an Event to all listeners.
        /// </summary>
        public static void InvokeEvent(string eventName, Module invocationSource)
        {
            if (events.ContainsKey(eventName))
                events[eventName].Invoke(invocationSource);
        }

        /// <summary>
        /// Registers an Action to be invoked when the specified Event is called. The key can be used to unregister this Action. (duplicate keys are allowed)
        /// </summary>
        public static void AddEventListener(string eventName, Action<Module> call, string listenerKey)
        {
            // create event if it does not exist yet
            if (!events.ContainsKey(eventName))
                events.Add(eventName, new Event<Module>());

            // add listener
            events[eventName].AddListener(call, listenerKey);
        }

        /// <summary>
        /// Unregisters all listeners with specified key from specified Event.
        /// </summary>
        public static void RemoveEventListener(string eventName, string listenerKey)
        {
            if (events.ContainsKey(eventName))
                events[eventName].RemoveListener(listenerKey);
        }

        /// <summary>
        /// Registers a new Modifier to be applied to Attributes, excluding duplicates.
        /// </summary>
        /// <returns>The new Modifier if added, otherwise the already existing duplicate.</returns>
        public static T AddModifier<T>(T modifier) where T : Modifier
        {
            T? existingDuplicate = (T?)modifiers.FirstOrDefault(exMod => exMod.GetType() == typeof(T) && exMod.IsDuplicateOf(modifier)); // (does this return null as default?)
            if (existingDuplicate == null)
            {
                modifiers.Add(modifier);

                // let all attributes check if the new modifier applies to them and perchance recalculate
                OnModifierPublished.Invoke(modifier);

                return modifier;
            }
            else
                return existingDuplicate;
        }

        public static void RemoveModifier(Modifier modifier)
        {
            modifiers.Remove(modifier);
        }

        /// <returns>All modifiers (does not return null)</returns>
        public static Modifier[] GetAllModifiers()
        {
            return modifiers.ToArray();
        }

        /// <returns>All modifiers that apply to specified attribute. (does not return null)</returns>
        public static Modifier[] GetApplicableModifiers(Attribute attribute)
        {
            return modifiers.Where(modifier => modifier.AppliesTo(attribute)).ToArray(); // (todo: increase search efficiency)
        }
    }
}
