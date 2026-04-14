using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace corasp_nyx.MonkeyModules
{
    /// <summary>
    /// Base Modifer class. Does not include Attribute modification. To modify Attribute&lt;T&gt;, use Modifier&lt;T&gt;.
    /// </summary>
    public abstract class Modifier : InteractiveComponent
    {
        /// <summary>The enforcers of this Modifier.</summary>
        protected List<object>? enforcers;

        /// <summary>Priority determining the place in Attribute modification order.</summary>
        public abstract int priority { get; protected set; }

        /// <summary>Requirements deeming which Attributes are affected.</summary>
        protected readonly ModifierRequirement[]? requirements;

        //public abstract Type affectedAttributeType { get; protected set; } // (remove?)

        /// <summary>This Event is called whenever a value used in the calculation of this Modifier changes. It is used to induce modified Attribute recalculations.</summary>
        public readonly Event<List<Attribute>> OnChange = new Event<List<Attribute>>();

        
        /// <param name="requirements">Try to enter requirements in order of least to most performance heavy verifications, to reduce processing load.</param>
        public Modifier(IEnumerable<ModifierRequirement> requirements)
        {
            this.requirements = requirements.ToArray();
        }

        /// <summary>
        /// Decommissions this Modifier when discarded by all enforcers, calling OnChange one last time, before cutting connections and marking it as decommissioned.
        /// </summary>
        public virtual void Discard(object discardingEnforcer)
        {
            if (discardingEnforcer != null)
                UnregisterEnforcer(discardingEnforcer);

            if ((enforcers?.Count ?? 0) == 0)
                Decommission();
        }

        /// <summary>
        /// Decommissions this Modifier, calling OnChange one last time, before cutting connections and marking it to not be used any further.
        /// </summary>
        public override void Decommission()
        {
            if (enforcers != null)
                UnregisterEnforcers(enforcers);
            GlobalManager.RemoveModifier(this);
            OnChange.Invoke(new List<Attribute>());
            OnChange.ClearListeners();

            base.Decommission();
        }

        /// <summary>
        /// Registers a class enforcing this Modifier to affect Attributes.
        /// </summary>
        /// <param name="enforcer">Usually a Module.</param>
        public virtual void RegisterEnforcer(object enforcer)
        {
            enforcers ??= new List<object>();
            if (!enforcers.Contains(enforcer))
                enforcers.Add(enforcer);
        }

        /// <summary>
        /// Registers classes using this Modifier to affect Attributes.
        /// </summary>
        /// <param name="enforcers">Usually Modules.</param>
        public virtual void RegisterEnforcers(IEnumerable<object> enforcers)
        {
            foreach (object user in enforcers)
                RegisterEnforcer(user);
        }

        /// <summary>
        /// Unregisters a class no longer enforcing this Modifier's existence.
        /// </summary>
        /// <param name="enforcer">Generally a Module.</param>
        public virtual void UnregisterEnforcer(object enforcer)
        {
            enforcers?.RemoveAll(registration => registration == enforcer);
        }

        /// <summary>
        /// Unregisters classes no longer enforcing this Modifier's existence.
        /// </summary>
        /// <param name="enforcers">Generally Modules.</param>
        public virtual void UnregisterEnforcers(IEnumerable<object> enforcers)
        {
            foreach (object enforcer in new List<object>(enforcers))
                UnregisterEnforcer(enforcer);
        }

        /// <returns>Enforcers of this Modifier (does not return null)</returns>
        public virtual object[] GetEnforcers()
        {
            return enforcers?.ToArray() ?? new object[0];
        }

        /// <summary>
        /// Called when calculation values changed, to trigger affected Attribute recalculation.
        /// </summary>
        /// <param name="changedAttributes">List of Attributes recalculated due to change.</param>
        protected virtual void NotifyOfVariableChange(List<Attribute> changedAttributes)
        {
            OnChange.Invoke(changedAttributes);
        }

        public virtual bool AppliesTo(Attribute attribute) => !decommissioned && (requirements?.All(requirement => requirement.IsApplicable(attribute)) ?? true);

        /// <returns>Whether Modifier Type, priorities, and requirements match. (Inheriting Modifiers should be comparing further parameters.)</returns>
        public virtual bool IsDuplicateOf(Modifier? other)
        {
            return other != null && GetType() == other.GetType() && priority == other.priority && requirements.SequenceEqual(other.requirements);
        }
    }

    /// <summary>
    /// Base modifier class, including value modification methods.
    /// </summary>
    /// <remarks>Modifiers should be replaced, instead of updated, whenever requirements or constant parameters need to be changed. Use Attribute references for dynamic calculations.</remarks>
    /// <typeparam name="T">Affected Attribute value Type.</typeparam>
    public abstract class Modifier<T> : Modifier
    {
        //public override Type affectedAttributeType { get; protected set; } = typeof(Attribute<T>); // (remove?)

        public Modifier(IEnumerable<ModifierRequirement> requirements) : base(requirements) { }

        /// <summary>
        /// Custom modifies specified attribute value.
        /// </summary>
        public abstract void Modify(ref T? value, Attribute attribute);

        /// <returns>Whether Modifier Type, priorities, requirements, and calculations match.</returns>
        public sealed override bool IsDuplicateOf(Modifier? other)
        {
#pragma warning disable CS8604 // Possible null reference argument. (handled by IsDuplicateOf method)
            return base.IsDuplicateOf(other) && MatchesCalculationOf(other as Modifier<T>);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        /// <returns>Whether Modifier modification calculations use the same values.</returns>
        protected abstract bool MatchesCalculationOf(Modifier<T> other);
    }
}
