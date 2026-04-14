using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace corasp_nyx.MonkeyModules
{
    /// <summary>
    /// Base Attribute class. Does not contain a value or modifiers.
    /// </summary>
    public abstract class Attribute : InteractiveComponent
    {
        /// <summary>The immutable non-unique identifier of this Attribute.</summary>
        public readonly string name;

        /// <summary>The users of this Attribute.</summary>
        protected List<object>? users;

        protected List<Modifier>? appliedModifiers;

        /// <summary>This Event is called whenever the value of this Attribute changes. It is used to induce dependant Modifier recalculations, and includes a chain of previously changed Attributes.</summary>
        public readonly Event<List<Attribute>> OnChange = new Event<List<Attribute>>();

        /// <summary>The amount of times this Attribute's value is allowed to be recalculated when included in a calculation dependecy recursion upon any value change.</summary>
        /// <remarks>More recursions may yield more accurate results, but increase performance costs. Requirement for this should be circumvented altogether by designing Modifiers with avoidance of such cases in mind.</remarks>
        protected const int maxRecursionsOnModCalculation = 3; // more recursions yield more accurate results but increase performance cost (necessity should be circumvented by good modifier design)

        protected const string calculationEventKeySuffix = "-Calculate";
        protected const string validationEventKeySuffix = "-Validate";

        public Attribute(string name)
        {
            this.name = name;

            // validate newly created modifiers
            GlobalManager.OnModifierPublished.AddListener((Modifier modifier) => { if (ValidateModifier(modifier)) NotifyOfModifierChange(new()); }, uid + validationEventKeySuffix);
        }

        /// <summary>
        /// Decommissions this Attribute when discarded by all users, calling OnChange one last time, before cutting connections and marking it as decommissioned.
        /// </summary>
        public virtual void Discard(object discardingUser)
        {
            if (discardingUser != null)
                UnregisterUser(discardingUser);

            if ((users?.Count ?? 0) == 0)
                Decommission();
        }

        /// <summary>
        /// Decommissions this Attribute, calling OnChange one last time, before cutting connections and marking it to not be used any further.
        /// </summary>
        public override void Decommission()
        {
            if (users != null)
                UnregisterUsers(users);
            OnChange.Invoke(new List<Attribute>() { this });
            OnChange.ClearListeners();

            base.Decommission();
        }

        /// <summary>
        /// Registers a class using this Attribute for detection by Modifiers.
        /// </summary>
        /// <param name="user">Usually a Module.</param>
        public virtual void RegisterUser(object user)
        {
            users ??= new List<object>();
            if (!users.Contains(user))
            {
                users.Add(user);

                // refresh modifiers after user change or perhaps for first time after creating attribute
                RetrieveModifiers();
                NotifyOfModifierChange(new());
            }
        }

        /// <summary>
        /// Registers classes using this Attribute for detection by Modifiers.
        /// </summary>
        /// <param name="user">Usually Modules.</param>
        public virtual void RegisterUsers(IEnumerable<object> users)
        {
            foreach (object user in users)
                RegisterUser(user);
        }

        /// <summary>
        /// Unregisters a class no longer using this Attribute.
        /// </summary>
        /// <param name="user">Generally a Module.</param>
        public virtual void UnregisterUser(object user)
        {
            users?.RemoveAll(registration => registration == user);

            // refresh modifiers after user change (todo: prevent this from firing one more time before decommission when being discarded?) (checking remaining user count here as well would be risky because discard is a virtual method)
            RetrieveModifiers();
            NotifyOfModifierChange(new());
        }

        /// <summary>
        /// Unregisters classes no longer using this Attribute.
        /// </summary>
        /// <param name="users">Generally Modules.</param>
        public virtual void UnregisterUsers(IEnumerable<object> users)
        {
            foreach (object user in new List<object>(users))
                UnregisterUser(user);
        }

        /// <returns>Users of this Attribute (does not return null)</returns>
        public virtual object[] GetUsers()
        {
            return users?.ToArray() ?? new object[0];
        }

        /// <summary>
        /// Removes all unapplicable Modifiers, without recalculating.
        /// </summary>
        protected virtual void DisposeModifiers()
        {
            // (remove unapplicable modifiers and unhook from their change events)
            if (appliedModifiers != null)
                foreach (Modifier modifier in new List<Modifier>(appliedModifiers))
                    if (!modifier.AppliesTo(this))
                    {
                        appliedModifiers.Remove(modifier);
                        modifier.OnChange.RemoveListener(uid + calculationEventKeySuffix);
                    }
        }

        /// <summary>
        /// Checks for new applicable Modifiers and saves them, without recalculating.
        /// </summary>
        protected virtual void RetrieveModifiers()
        {
            // retrieve new applicable modifiers
            IEnumerable<Modifier> newModifiers = appliedModifiers != null ? GlobalManager.GetApplicableModifiers(this).Where(modifier => !appliedModifiers.Contains(modifier)) : GlobalManager.GetApplicableModifiers(this);

            if (newModifiers.Count() > 0)
            {
                // register for update on change in new modifiers
                foreach (Modifier modifier in newModifiers)
                    modifier.OnChange.AddListener(NotifyOfModifierChange, uid + calculationEventKeySuffix);

                // save new modifiers
                if (appliedModifiers == null)
                    appliedModifiers = new List<Modifier>(newModifiers);
                else
                    appliedModifiers.AddRange(newModifiers);
            }
        }

        /// <summary>
        /// Checks if the given Modifier is applicable, and if need be, applies or unapplies it.
        /// </summary>
        /// <returns>Whether the Modifier has been either applied or unapplied.</returns>
        protected virtual bool ValidateModifier(Modifier modifier)
        {
            // check if this modifier is not already applied
            if (!appliedModifiers?.Contains(modifier) ?? true)
            {
                // check if this modifier should be applied
                if (modifier.AppliesTo(this))
                {
                    // register for update on change in new modifier
                    modifier.OnChange.AddListener(NotifyOfModifierChange, uid + calculationEventKeySuffix);

                    // save new modifier
                    if (appliedModifiers == null)
                        appliedModifiers = new List<Modifier>() { modifier };
                    else
                        appliedModifiers.Add(modifier);

                    return true;
                }
            }
            // check if this modifier should be unapplied (if applied)
            else if (!modifier.AppliesTo(this))
            {
                // unregister modifier
                appliedModifiers?.Remove(modifier);
                modifier.OnChange.RemoveListener(uid + calculationEventKeySuffix);

                return true;
            }

            // if applied and still applicable or not applied and still not applicable
            return false;
        }

        /// <summary>
        /// Called when modifiers changed, to trigger value recalculation.
        /// </summary>
        /// <param name="changedAttributes">List of Attributes recalculated due to change.</param>
        public virtual void NotifyOfModifierChange(List<Attribute> changedAttributes)
        {
            // recalculate attribute value and notify sourcing modifiers on change, as long as this attribute has not been recalculated through a single change too many times
            if (changedAttributes.FindAll(attribute => attribute == this).Count < maxRecursionsOnModCalculation)
            {
                // dispose decommissioned modifiers
                DisposeModifiers();

                // check if value has changed
                if (Calculate())
                {
                    changedAttributes.Add(this);
                    OnChange.Invoke(changedAttributes);
                }
            }
        }

        /// <summary>
        /// Apply current modifiers to Attribute.
        /// </summary>
        /// <returns>Whether calculation has caused a change.</returns>
        protected abstract bool Calculate();
    }

    // (todo: maybe add an unmodifiable attribute for solely broadcasting purposes? move modifier methods back into child classes?)

    /// <summary>
    /// Base Attribute class including a value and modifiers.
    /// </summary>
    /// <typeparam name="T">Type of Attribute value.</typeparam>
    public class Attribute<T> : Attribute
    {
        /// <summary>The calculated, up-to-date value of this Attribute.</summary>
        protected T? value; // (changed to C# version 9.0 to allow for ambiguous nullable types of either value or reference)
        /// <summary>The unmodified base value of this Attribute, allowing easier simple changes without excessive Modifier load.</summary>
        protected T? baseValue;

        public Attribute(string name, T? baseValue = default) : base(name)
        {
            value = baseValue;
            this.baseValue = baseValue;
        }

        /// <returns>The calculated, up-to-date value of this Attribute.</returns>
        public T? GetValue() => value;

        /// <returns>The unmodified base value of this Attribute.</returns>
        public T? GetBaseValue() => baseValue;

        public Type GetValueType() => typeof(T);

        /// <summary>Apply a new base value, without requiring a modifier.</summary>
        public virtual void SetBaseValue(T? newBaseValue)
        {
            // cache value to check and recalculate if changed after applying new value
            T? cachedValue = baseValue;

            // set new base value
            baseValue = newBaseValue;

            // recalculate if base value changed
            if (!(cachedValue?.Equals(baseValue) ?? baseValue == null) && Calculate())
                OnChange.Invoke(new List<Attribute>() { this });
        }

        /// <summary>
        /// Apply current modifiers to Attribute value.
        /// </summary>
        /// <returns>Whether value has changed after calculation.</returns>
        protected override bool Calculate()
        {
            // cache value to inspect change after recalculation
            T? cachedValue = value;

            // modify value in order of modifier priority
            value = baseValue;
            if (appliedModifiers != null)
                foreach (Modifier<T> modifier in new List<Modifier>(appliedModifiers.OrderBy(modifier => modifier.priority))) // (does this implicit conversion work??)
                    modifier.Modify(ref value, this);

            return !(cachedValue?.Equals(value) ?? value == null);
        }
    }
}
