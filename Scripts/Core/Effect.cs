using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace TDP.InteractiveComponents
{
    /// <summary>
    /// Base Effect class. Does not include Attributes.
    /// </summary>
    public abstract class Effect : InteractiveComponent
    {
        public EffectContainer? containedEffects;

        /*private List<Modifier>? createdModifiers;

        protected void CreateModifier(Modifier modifier)
        {
            if (ECSManager.AddModifier(modifier) == modifier)
            {
                createdModifiers ??= new List<Modifier>();
                createdModifiers.Add(modifier); // only save modifier reference if it is not a duplicate of one already created
            }
        }

        protected void DestroyModifier(Modifier modifier)
        {
            ECSManager.RemoveModifier(modifier);
        }*/

        //internal virtual void NotifyOfAdditionToContainer() { }

        /*/// <summary>
        /// Adds a Container to this Effect, merging it with existing ones.
        /// </summary>
        /// <typeparam name="T">Container content Type.</typeparam>
        public virtual void AddContainer<T>(Container<T> container)
        {
            containers ??= new List<Container>();

            // adds container if none of the same type already exist
            if (!containers.Any(container => container is Container<T>))
                containers.Add(container);
            // otherwise adds content to existing container of same type
            else if (container.GetContent().Length != 0)
                ((Container<T>)containers.First(container => container is Container<T>)).AddContent(container);
        }

        /// <summary>
        /// Removes all Containers of certain Type from this Effect.
        /// </summary>
        /// <typeparam name="T">Container content Type.</typeparam>
        public virtual void RemoveContainer<T>()
        {
            containers?.RemoveAll(container => container is Container<T>);
            // (deleting container list if empty here could save memory but increase performance cost if frequently populated and drained)
        }

        /// <summary>
        /// Adds content to an existing or new container on this Effect.
        /// </summary>
        public virtual void AddContainerContent<T>(T content)
        {
            // check if content is null
            if (content == null)
            {
                MessageOutput.Log($"Cannot add null as content to {GetType().Name} container!");
                return;
            }

            AddContainer(new Container<T>());
        }

        /// <summary>
        /// Removes specified content from all Effect containers.
        /// </summary>
        public virtual void RemoveContainerContent<T>(T content)
        {
            if (content != null && containers != null)
                foreach (Container<T> container in containers.Where(container => container is Container<T>))
                    container.RemoveContent(content);
        }*/
    }

    /// <summary>
    /// Base Effect class including Attributes.
    /// </summary>
    public abstract class LoadedEffect : Effect
    {
        /// <summary>
        /// References to all Attributes used by this Effect. Should not contain duplicates. Recommended to use public access methods when inheriting.
        /// </summary>
        protected readonly Dictionary<string, Attribute> attributes = new Dictionary<string, Attribute>();

        /// <summary>
        /// Stores an Attribute to be used by this Effect, if one with the same name is not already in usage.
        /// </summary>
        /// <typeparam name="T">Attribute class.</typeparam>
        public virtual void AddAttribute<T>(T attribute) where T : Attribute
        {
            // avoid duplicates by checking immutable name
            if (!attributes.ContainsKey(attribute.name))
            {
                attribute.RegisterUser(this);
                attributes.Add(attribute.name, attribute);
            } // (todo: at warning message if contained attribute of same name is of a different type)
        }

        /// <summary>
        /// Unregisters an Attribute used by this Effect, without decommissioning it.
        /// </summary>
        /// <typeparam name="T">Attribute class.</typeparam>
        public virtual void RemoveAttribute<T>(T attribute) where T : Attribute
        {
            attribute.UnregisterUser(this);
            attributes.Remove(attribute.name);
        }

        /// <summary>
        /// Unregisters an Attribute used by this Effect and decommissions it from further use, if not currently used elsewhere.
        /// </summary>
        /// <typeparam name="T">Attribute class.</typeparam>
        public virtual void DiscardAttribute<T>(T attribute) where T : Attribute
        {
            attribute.UnregisterUser(this);
            attributes.Remove(attribute.name);
        }

        /// <returns>All Attributes used by this Effect (does not return null)</returns>
        public virtual Attribute[] GetAttributes()
        {
            return attributes.Values.ToArray();
        }

        /// <returns>Whether this Effect uses an Attribute with the specified name.</returns>
        public virtual bool HasAttribute(string name)
        {
            return attributes.ContainsKey(name);
        }

        /// <summary>
        /// Decommission all Attributes used solely by this Effect. Recommended to do this when deleting an Effect.
        /// </summary>
        public virtual void DiscardAllAttributes()
        {
            foreach (Attribute attribute in GetAttributes())
                DiscardAttribute(attribute);
        }

        /// <returns>Attribute with specified name, if used by this Effect, otherwise null.</returns>
        /// <param name="ignoreWarning">Suppresses log output on failure.</param>
        public virtual Attribute? GetAttribute(string name, bool ignoreWarning = false) => GetAttribute<Attribute>(name, ignoreWarning);

        /// <returns>Attribute with specified Type and name, if used by this Effect, otherwise null.</returns>
        /// <param name="ignoreWarning">Suppresses log output on failure.</param>
        public virtual T? GetAttribute<T>(string name, bool ignoreWarning = false) where T : Attribute
        {
            // check if an attribute with given name is stored
            if (!attributes.ContainsKey(name))
            {
                if (!ignoreWarning)
                    MessageOutput.Log($"Cannot access {typeof(T).Name} '{name}' because {typeof(LoadedEffect).Name} does not have an Attribute of that name.");
                return null;
            }

            // check if the stored attribute with given name is the correct type
            if (!(attributes[name] is T))
            {
                if (!ignoreWarning)
                    MessageOutput.Log($"Cannot access {typeof(T).Name} '{name}' because the Attribute of that name used by {typeof(LoadedEffect).Name} is of the incompatible Type {attributes[name].GetType().Name}.");
                return null;
            }

            return (T)attributes[name];
        }
    }
}
