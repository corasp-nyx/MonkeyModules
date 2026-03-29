using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace TDP.InteractiveComponents
{
    /// <summary>
    /// Base Modifer class. Does not include Attribute modification. To modify Attribute&lt;T&gt;, use Modifier&lt;T&gt;.
    /// </summary>
    public abstract class Modifier : ECSComponent
    {
        /// <summary>Priority determining the place in Attribute modification order.</summary>
        public abstract int priority { get; protected set; }

        /// <summary>Requirements deeming which Attributes are affected.</summary>
        protected readonly ModifierRequirement[]? requirements;

        //public abstract Type affectedAttributeType { get; protected set; } // (remove?)

        /// <summary>This Event is called whenever a value used in the calculation of this Modifier changes. It is used to induce modified Attribute recalculations.</summary>
        public readonly Event<List<Attribute>> OnChange = new Event<List<Attribute>>();

        /// <summary>True if this Modifier has been decommissioned.</summary>
        public bool decommissioned { get; protected set; }

        
        /// <param name="requirements">Try to enter requirements in order of least to most performance heavy verifications, to reduce processing load.</param>
        public Modifier(IEnumerable<ModifierRequirement> requirements)
        {
            this.requirements = requirements.ToArray();
        }

        /// <summary>
        /// Decommissions this Modifier, calling OnChange one last time, before cutting connections and marking it to not be used any further.
        /// </summary>
        public virtual void Decommission()
        {
            decommissioned = true;
            ECSManager.RemoveModifier(this);
            OnChange.Invoke(new List<Attribute>());
            OnChange.ClearListeners();
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
