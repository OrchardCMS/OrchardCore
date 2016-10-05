using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace Orchard.Environment.Cache
{
    public class DefaultTagCache : ITagCache
    {
        private const string CacheKey = nameof(DefaultTagCache);

        private readonly ConcurrentDictionary<string, HashSet<string>> _dictionary;
        private readonly ITagRemovedEventHandler _tagRemovedEventHandler;

        public DefaultTagCache(ITagRemovedEventHandler tagRemovedEventHandler, IMemoryCache memoryCache)
        {
            // We use the memory cache as the state holder and keep this class transient as it has
            // dependencies on non-singletons

            if(!memoryCache.TryGetValue(CacheKey, out _dictionary))
            {
                _dictionary = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                memoryCache.Set(CacheKey, _dictionary);
            }
            
            _tagRemovedEventHandler = tagRemovedEventHandler;
        }

        public void Tag(string key, params string[] tags)
        {
            foreach (var tag in tags)
            {
                var set = _dictionary.GetOrAdd(tag, x => new HashSet<string>());

                lock (set)
                {
                    set.Add(key);
                }
            }
        }

        public IEnumerable<string> GetTaggedItems(string tag)
        {
            HashSet<string> set;
            if (_dictionary.TryGetValue(tag, out set))
            {
                lock (set)
                {
                    return set;
                }
            }

            return Enumerable.Empty<string>();
        }

        public void RemoveTag(string tag)
        {
            HashSet<string> set;

            if (_dictionary.TryRemove(tag, out set))
            {
                _tagRemovedEventHandler.TagRemoved(tag, set);
            }
        }
    }
}
