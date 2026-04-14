using System;
using System.Collections.Generic;

#nullable enable
namespace corasp_nyx.MonkeyModules
{
    /// <summary>
    /// Container used to store groups of effects in a hierarchy.
    /// </summary>
    public class EffectContainer : InteractiveComponent // (maybe implement IList, IEnumerable, etc.?)
    {
        protected List<Module>? effects;

        /// <summary>
        /// Adds Effect. (does not exclude duplicates)
        /// </summary>
        public void AddEffect(Module effect)
        {
            this.effects ??= new List<Module>();

            this.effects.Add(effect);
        }

        /// <summary>
        /// Adds range of Effects. (does not exclude duplicates)
        /// </summary>
        public void AddEffects(IEnumerable<Module> effects)
        {
            this.effects ??= new List<Module>();

            this.effects.AddRange(effects);
        }

        /// <summary>
        /// Adds all Effects from another container. (does not exclude duplicates)
        /// </summary>
        public void AddEffects(EffectContainer container)
        {
            this.effects ??= new List<Module>();

            this.effects.AddRange(container.GetEffects());
        }

        /// <summary>
        /// Removes first occurrence of specified Effect.
        /// </summary>
        public void RemoveEffect(Module effect)
        {
            this.effects?.Remove(effect);
        }

        /// <summary>
        /// Removes all occurrences of specified Effects.
        /// </summary>
        public void RemoveEffects(IEnumerable<Module> effects)
        {
            if (this.effects != null)
                foreach (Module effect in effects)
                    this.effects.RemoveAll(e => e == effect);
        }

        /// <summary>
        /// Removes all matching Effects.
        /// </summary>
        public void RemoveAllEffects(Predicate<Module> match)
        {
            this.effects?.RemoveAll(match);
        }

        /// <summary>
        /// Removes all Effects.
        /// </summary>
        public void ClearEffects()
        {
            effects?.Clear();
        }

        /// <returns>Effects in this container (does not return null)</returns>
        public Module[] GetEffects()
        {
            return effects?.ToArray() ?? new Module[0];
        }
    }
}