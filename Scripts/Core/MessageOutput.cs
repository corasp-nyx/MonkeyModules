using System;
using System.Collections.Generic;

#nullable enable
namespace TDP.InteractiveComponents
{
    public static class MessageOutput
    {
        public static Event<string> OnMessage { get; private set; }

        static MessageOutput()
        {
            OnMessage ??= new Event<string>(); // (the null check might cause problems because these types are not nullable. it may be provisionally allowed before exiting constructor though)
        }

        /// <summary>
        /// Output an internal message to public listeners.
        /// </summary>
        internal static void Log(string message)
        {
            // pass on message without checking for exceptions as these would not be able to be passed on in that case
            OnMessage?.Invoke(message);
        }
    }
}
