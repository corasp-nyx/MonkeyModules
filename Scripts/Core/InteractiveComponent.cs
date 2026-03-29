using System;

namespace TDP.InteractiveComponents
{
    public abstract class InteractiveComponent
    {
        /// <summary>Immutable unique identifier of this module, always newly generated at runtime.</summary>
        public readonly string uid = DateTime.UtcNow.ToString() + Guid.NewGuid().ToString();
    }
}
