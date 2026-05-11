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
    /// A conditional requirement used to filter which Attributes are affected by a Modifier.
    /// </summary>
    public interface ModifierRequirement
    {
        /// <summary>See struct summary.</summary>
        public abstract bool IsApplicable(Attribute attribute);
    }

    /// <summary>
    /// Custom requirement that is evaluated individually. (Performance heavy)
    /// </summary>
    public readonly struct ModifierCustomRequirement : ModifierRequirement // performance heavy
    {
        public readonly Predicate<Attribute> requirement;

        public ModifierCustomRequirement(Predicate<Attribute> match)
        {
            requirement = match;
        }

        public bool IsApplicable(Attribute attribute) => requirement(attribute);
    }

    /// <summary>
    /// Requires modified Attributes to have specified Name.
    /// </summary>
    public readonly struct ModifierAttributeNameRequirement : ModifierRequirement
    {
        public readonly string attributeName;

        public ModifierAttributeNameRequirement(string attributeName)
        {
            this.attributeName = attributeName;
        }

        public bool IsApplicable(Attribute attribute) => attribute.name == attributeName;
    }

    /// <summary>
    /// Requires modified Attribute to have specified id.
    /// </summary>
    public readonly struct ModifierAttributeIdRequirement : ModifierRequirement
    {
        public readonly string attributeId;

        public ModifierAttributeIdRequirement(string attributeId)
        {
            this.attributeId = attributeId;
        }

        public bool IsApplicable(Attribute attribute) => attribute.uid == attributeId;
    }

    /// <summary>
    /// Requires modified Attributes to be of specified Type.
    /// </summary>
    public readonly struct ModifierAttributeTypeRequirement : ModifierRequirement
    {
        public readonly Type type;

        public ModifierAttributeTypeRequirement(Type type)
        {
            this.type = type;
        }

        public bool IsApplicable(Attribute attribute)
        {
            // copy value locally in struct method for use in lambda expression
            Type type = this.type;

            // returns true if Attribute is of the required Type
            return attribute.GetType().IsAssignableFrom(type);
        }
    }

    /// <summary>
    /// Requires modified Attributes to have a user of specified Type.
    /// </summary>
    public readonly struct ModifierAttributeUserTypeRequirement : ModifierRequirement
    {
        public readonly Type userType;

        public ModifierAttributeUserTypeRequirement(Type userType)
        {
            this.userType = userType;
        }

        public bool IsApplicable(Attribute attribute)
        {
            // copy value locally in struct method for use in lambda expression
            Type userType = this.userType;

            // returns true if any user is of the required Type
            return attribute.GetUsers().Any(user => user.GetType().IsAssignableFrom(userType));
        }
    }

    /// <summary>
    /// Requires modified Attributes to have a ModularItem user with specified id.
    /// </summary>
    public readonly struct ModifierAttributeUserIdRequirement : ModifierRequirement
    {
        public readonly string userId;

        public ModifierAttributeUserIdRequirement(string userId)
        {
            this.userId = userId;
        }

        public bool IsApplicable(Attribute attribute)
        {
            // copy value locally in struct method for use in lambda expression
            string userId = this.userId;

            // returns true if any user inherits from ModularItem and has the specified id
            return attribute.GetUsers().Any(user => ((user as InteractiveComponent)?.uid ?? "") == userId);
        }
    }

    /*/// <summary>
    /// Requires modified Attributes to be used by an Effect using a collection Attribute containing specified entry. (For example a keyword.)
    /// </summary>
    public readonly struct ModifierEffectCollectionEntryRequirement : ModifierRequirement
    {
        public readonly string attributeName;
        public readonly dynamic? attributeEntry;

        public ModifierEffectCollectionEntryRequirement(string attributeName, dynamic? attributeEntry)
        {
            this.attributeName = attributeName;
            this.attributeEntry = attributeEntry;
        }

        public bool IsApplicable<T>(Attribute<T> attribute) where T : struct
        {
            string attributeName = this.attributeName;
            dynamic? attributeEntry = this.attributeEntry;

            // returns true if any Attribute used by Attribute user is a collection and contains required entry
            return attribute.GetUsers().Any(user => (user as LoadedEffect)?.GetAttributes().Any(attributeB =>
            {
                if (attributeB.name == attributeName)
                    foreach (T entry in (attributeB as Attribute<IEnumerable<T>>)?.GetValue() ?? new List<T>())
                        if (entry == attributeEntry)
                            return true;
                return false;
            }) ?? false);
        }
    }*/
}
