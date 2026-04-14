using System;

namespace corasp_nyx.MonkeyModules
{
    public abstract class InteractiveComponent
    {
        /// <summary>Immutable unique identifier of this component, always newly generated at runtime.</summary>
        public readonly string uid = DateTime.UtcNow.ToString() + Guid.NewGuid().ToString();

        /// <summary>True if this Component has been decommissioned and should not be used further.</summary>
        public bool decommissioned { get; protected set; }

        public readonly Event<InteractiveComponent> OnDecommission =  new Event<InteractiveComponent>();

        protected const string decommissionEventKeySuffix = "-Decommission";

        public virtual void Decommission()
        {
            decommissioned = true;
            OnDecommission.Invoke(this);
        }
    }
}
