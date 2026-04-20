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
    /// <summary>
    /// Invocable Event with a single payload.
    /// </summary>
    /// <typeparam name="T">Payload Type.</typeparam>
    public class Event<T>
    {
        /// <summary>The Actions to be invoked when this Event is called. Contains revocable keys paired with single-payload Actions.</summary>
        protected List<KeyValuePair<string, Action<T>>>? listeners;

        /// <summary>
        /// Invokes listening Actions and passes along a Payload.
        /// </summary>
        public void Invoke(T payload)
        {
            if (listeners != null)
                foreach (Action<T> call in new List<Action<T>>(listeners.Select(listener => listener.Value)))
                    call.Invoke(payload);
        }

        /// <summary>
        /// Registers an Action to be invoked when this Event is called. The key can be used to unregister this Action. (duplicate keys are allowed)
        /// </summary>
        public void AddListener(Action<T> call, string key)
        {
            listeners ??= new List<KeyValuePair<string, Action<T>>>();

            // register Action
            listeners.Add(new KeyValuePair<string, Action<T>>(key, call));
        }

        /// <summary>
        /// Registers an Action to be invoked when this Event is called, ignoring Payload. The key can be used to unregister this Action. (duplicate keys are allowed)
        /// </summary>
        public void AddListener(Action call, string key)
        {
            // register Action ignoring payload
            AddListener((source) => call.Invoke(), key);
        }

        /// <summary>
        /// Unregisters all listeners with specified key.
        /// </summary>
        public void RemoveListener(string key)
        {
            listeners?.RemoveAll(listener => listener.Key == key);
        }

        public void ClearListeners()
        {
            listeners = null;
        }
    }
}
