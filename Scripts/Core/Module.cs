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
    /// Base Module class. Does not include Attributes.
    /// </summary>
    public abstract class Module : InteractiveComponent
    {
        public readonly List<Module> subModules = new List<Module>();

        /// <returns>The first subordinate Module of this Module with specifide type. (faster than GetSubModules())</returns>
        public virtual T? GetSubModule<T>(bool recursive = false) where T : Module => GetSubModule(typeof(T)) as T;

        /// <returns>The first subordinate Module of this Module with specifide type. (faster than GetSubModules())</returns>
        public virtual Module? GetSubModule(Type type, bool recursive = false)
        {
            foreach (Module subModule in new List<Module>(subModules))
                if (subModule.GetType() == type)
                    return subModule;
                else if (recursive)
                    foreach (Module subSubModule in new List<Module>(subModule.subModules))
                    {
                        Module? target = subSubModule.GetSubModule(type, recursive);
                        if (target != null)
                            return target;
                    }

            return null;
        }

        /// <returns>All subordinate Modules of this Module with specified inheritance. (does not return null)</returns>
        public virtual T[] GetSubModules<T>(bool recursive = false) where T : class => (recursive ? subModules.OfType<T>().Concat(subModules.SelectMany(module => module.GetSubModules<T>(recursive))) : subModules.OfType<T>()).ToArray();

        /// <returns>All subordinate Modules of this Module with specified type. (does not return null)</returns>
        public virtual Module[] GetSubModules(Type type, bool recursive = false) => (recursive ? subModules.Concat(subModules.SelectMany(module => module.GetSubModules(type, recursive))) : subModules).Where(module => module.GetType().IsAssignableFrom(type)).ToArray();


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
    /// Base Module class including Attributes.
    /// </summary>
    public abstract class LoadedModule : Module
    {
        /// <summary>
        /// References to all Attributes used by this Module. Should not contain duplicates. Recommended to use public access methods when inheriting.
        /// </summary>
        protected readonly Dictionary<string, Attribute> attributes = new Dictionary<string, Attribute>();

        protected const string discardingEventKeySuffix = "-Discarding";

        /// <summary>
        /// Stores an Attribute to be used by this Module, if one with the same name is not already in usage.
        /// </summary>
        /// <typeparam name="T">Attribute class.</typeparam>
        /// <param name="discardOnDecommission">Whether to discard this Attribute when this Module is decommissioned.</param>
        public virtual void AddAttribute<T>(T attribute, bool discardOnDecommission = true) where T : Attribute
        {
            // avoid duplicates by checking immutable name
            if (!attributes.ContainsKey(attribute.name))
            {
                attribute.RegisterUser(this);
                attributes.Add(attribute.name, attribute);

                if (discardOnDecommission)
                    OnDecommission.AddListener(attribute.Discard, attribute.uid + discardingEventKeySuffix); // (does not remove them from local dictionary, although that should be irrelevant at that point) (creates more bloat than discarding all on decommission, but retains easier customisation)
            }
            else if (attributes[attribute.name] is not T)
                MessageOutput.Log($"Cannot add {typeof(T).Name} '{attribute.name}' because {typeof(LoadedModule).Name} already uses an Attribute with that name but of a different Type: {attributes[attribute.name].GetType().Name}.");
        }

        /// <summary>
        /// Globally registers a new Modifier to be applied to Attributes, excluding duplicates.
        /// </summary>
        /// <returns>The new Modifier if added, otherwise the already existing duplicate.</returns>
        /// <param name="discardOnDecommission">Whether to discard this Modifier when this Module is decommissioned. (Can be useful to disable if the same Modifier would be created and decommissioned repeatedly otherwise)</param>
        protected virtual T AddModifier<T>(T modifier, bool discardOnDecommission = true) where T : Modifier
        {
            if (discardOnDecommission)
                OnDecommission.AddListener(modifier.Decommission, modifier.uid + discardingEventKeySuffix);

            return GlobalManager.AddModifier(modifier);
        }

        /// <summary>
        /// Unregisters an Attribute used by this Module, without decommissioning it.
        /// </summary>
        /// <typeparam name="T">Attribute class.</typeparam>
        public virtual void RemoveAttribute<T>(T attribute) where T : Attribute
        {
            attribute.UnregisterUser(this);
            attributes.Remove(attribute.name);
        }

        /// <summary>
        /// Unregisters an Attribute used by this Module and decommissions it from further use, if not currently used elsewhere.
        /// </summary>
        /// <typeparam name="T">Attribute class.</typeparam>
        public virtual void DiscardAttribute<T>(T attribute) where T : Attribute
        {
            attribute.UnregisterUser(this);
            attributes.Remove(attribute.name);
        }

        /// <returns>All Attributes used by this Module (does not return null)</returns>
        public virtual Attribute[] GetAttributes()
        {
            return attributes.Values.ToArray();
        }

        /// <returns>Whether this Module uses an Attribute with the specified name.</returns>
        public virtual bool HasAttribute(string name)
        {
            return attributes.ContainsKey(name);
        }

        /// <summary>
        /// Discards all Attributes used by this Module, decommissioning them if not used elsewhere.
        /// </summary>
        public virtual void DiscardAllAttributes()
        {
            foreach (Attribute attribute in GetAttributes())
                DiscardAttribute(attribute);
        }

        /// <returns>Attribute with specified name, if used by this Module, otherwise null.</returns>
        /// <param name="ignoreWarning">Suppresses log output on failure.</param>
        public virtual Attribute? GetAttribute(string name, bool ignoreWarning = false) => GetAttribute<Attribute>(name, ignoreWarning);

        /// <returns>Attribute with specified Type and name, if used by this Module, otherwise null.</returns>
        /// <param name="ignoreWarning">Suppresses log output on failure.</param>
        public virtual T? GetAttribute<T>(string name, bool ignoreWarning = false) where T : Attribute
        {
            // check if an attribute with given name is stored
            if (!attributes.ContainsKey(name))
            {
                if (!ignoreWarning)
                    MessageOutput.Log($"Cannot access {typeof(T).Name} '{name}' because {typeof(LoadedModule).Name} does not have an Attribute of that name.");
                return null;
            }

            // check if the stored attribute with given name is the correct type
            if (!(attributes[name] is T))
            {
                if (!ignoreWarning)
                    MessageOutput.Log($"Cannot access {typeof(T).Name} '{name}' because the Attribute of that name used by {typeof(LoadedModule).Name} is of the incompatible Type {attributes[name].GetType().Name}.");
                return null;
            }

            return (T)attributes[name];
        }
    }
}
