using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Cache
{
    public class CacheContext
    {
        private HashSet<string> _contexts;
        private HashSet<string> _tags;
        private string _cacheId;
        private TimeSpan? _duration;

        public CacheContext(string cacheId)
        {
            _cacheId = cacheId;
        }

        /// <summary>
        /// Defines the absolute time the shape should be cached for.
        /// If not called a sliding value will be used.
        /// </summary>
        public CacheContext During(TimeSpan duration)
        {
            _duration = duration;
            return this;
        }

        /// <summary>
        /// Defines a dimension to cache the shape for. For instance by using <code>"user"</code>
        /// each user will get a different value.
        /// </summary>
        public CacheContext AddContext(params string[] contexts)
        {
            if (_contexts == null)
            {
                _contexts = new HashSet<string>();
            }

            foreach (var context in contexts)
            {
                _contexts.Add(context);
            }

            return this;
        }

        /// <summary>
        /// Removes a specific context.
        /// </summary>
        public CacheContext RemoveContext(string context)
        {
            if (_contexts != null)
            {
                _contexts.Remove(context);
            }

            return this;
        }

        /// <summary>
        /// Defines a dimension that will invalidate the cache entry when it changes.
        /// For instance by using <code>"features"</code> every time the list of features
        /// will change the value of the cache will be invalidated.
        /// </summary>
        public CacheContext AddDependency(params string[] context)
        {
            return AddContext(context);
        }

        /// <summary>
        /// Removes a specific dependency.
        /// </summary>
        public CacheContext RemoveDependency(string context)
        {
            return RemoveContext(context);
        }

        public CacheContext AddTag(params string[] tags)
        {
            if (_tags == null)
            {
                _tags = new HashSet<string>();
            }

            foreach (var tag in tags)
            {
                _tags.Add(tag);
            }

            return this;
        }

        public CacheContext RemoveTag(string tag)
        {
            if (_tags != null)
            {
                _tags.Remove(tag);
            }

            return this;
        }

        public string CacheId => _cacheId;
        public IEnumerable<string> Contexts => _contexts ?? Enumerable.Empty<string>();
        public IEnumerable<string> Tags => _tags ?? Enumerable.Empty<string>();
        public TimeSpan? Duration => _duration;

    }
}
