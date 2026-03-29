using System;
using System.Collections.Generic;

#nullable enable
namespace TDP.InteractiveComponents
{
    /// <summary>
    /// Container used to store groups of effects in a hierarchy.
    /// </summary>
    public class EffectContainer : ECSComponent // (maybe implement IList, IEnumerable, etc.?)
    {
        protected List<Effect>? effects;

        /// <summary>
        /// Adds Effect. (does not exclude duplicates)
        /// </summary>
        public void AddEffect(Effect effect)
        {
            this.effects ??= new List<Effect>();

            this.effects.Add(effect);
        }

        /// <summary>
        /// Adds range of Effects. (does not exclude duplicates)
        /// </summary>
        public void AddEffects(IEnumerable<Effect> effects)
        {
            this.effects ??= new List<Effect>();

            this.effects.AddRange(effects);
        }

        /// <summary>
        /// Adds all Effects from another container. (does not exclude duplicates)
        /// </summary>
        public void AddEffects(EffectContainer container)
        {
            this.effects ??= new List<Effect>();

            this.effects.AddRange(container.GetEffects());
        }

        /// <summary>
        /// Removes first occurrence of specified Effect.
        /// </summary>
        public void RemoveEffect(Effect effect)
        {
            this.effects?.Remove(effect);
        }

        /// <summary>
        /// Removes all occurrences of specified Effects.
        /// </summary>
        public void RemoveEffects(IEnumerable<Effect> effects)
        {
            if (this.effects != null)
                foreach (Effect effect in effects)
                    this.effects.RemoveAll(e => e == effect);
        }

        /// <summary>
        /// Removes all matching Effects.
        /// </summary>
        public void RemoveAllEffects(Predicate<Effect> match)
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
        public Effect[] GetEffects()
        {
            return effects?.ToArray() ?? new Effect[0];
        }
    }
}