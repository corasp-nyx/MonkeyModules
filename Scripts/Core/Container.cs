/*using System;
using System.Collections.Generic;

namespace TDP.ModularEffects
{
    public interface Container { }

    /// <summary>
    /// Container used to store groups of objects in a hierarchy. Not meant to be identified by instance.
    /// </summary>
    /// <typeparam name="T">Content Type.</typeparam>
    public class Container<T> : Container // (maybe implement IList, IEnumerable, etc.?)
    {
        protected List<T>? content;

        /// <summary>
        /// Adds content.
        /// </summary>
        public void AddContent(T content)
        {
            this.content ??= new List<T>();

            this.content.Add(content);
        }

        /// <summary>
        /// Adds range of content.
        /// </summary>
        public void AddContent(IEnumerable<T> content)
        {
            this.content ??= new List<T>();

            this.content.AddRange(content);
        }

        /// <summary>
        /// Adds all content from another container.
        /// </summary>
        public void AddContent(Container<T> container)
        {
            this.content ??= new List<T>();

            this.content.AddRange(container.GetContent());
        }

        /// <summary>
        /// Removes first occurrence of specified content.
        /// </summary>
        public void RemoveContent(T content)
        {
            this.content?.Remove(content);
        }

        /// <summary>
        /// Removes all matching content.
        /// </summary>
        public void RemoveAllContent(Predicate<T> match)
        {
            this.content?.RemoveAll(match);
        }

        /// <summary>
        /// Removes all content.
        /// </summary>
        public void ClearContent()
        {
            content?.Clear();
        }

        /// <returns>Contents of this container (does not return null)</returns>
        public T[] GetContent()
        {
            return content?.ToArray() ?? new T[0];
        }

        public Type GetContentType() // redundant
        {
            return typeof(T);
        }
    }
}
*/