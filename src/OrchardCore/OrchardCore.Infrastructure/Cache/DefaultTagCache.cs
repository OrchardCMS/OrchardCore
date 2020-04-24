using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Cache
{
    public class DefaultTagCache : ITagCache
    {
        private const string CacheKey = nameof(DefaultTagCache);

        private readonly ConcurrentDictionary<string, HashSet<string>> _dictionary;
        private readonly IEnumerable<ITagRemovedEventHandler> _tagRemovedEventHandlers;
        private readonly ILogger _logger;

        public DefaultTagCache(
            IEnumerable<ITagRemovedEventHandler> tagRemovedEventHandlers,
            IMemoryCache memoryCache,
            ILogger<DefaultTagCache> logger)
        {
            // We use the memory cache as the state holder and keep this class transient as it has
            // dependencies on non-singletons

            if (!memoryCache.TryGetValue(CacheKey, out _dictionary))
            {
                _dictionary = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                memoryCache.Set(CacheKey, _dictionary);
            }

            _tagRemovedEventHandlers = tagRemovedEventHandlers;
            _logger = logger;
        }

        public Task TagAsync(string key, params string[] tags)
        {
            foreach (var tag in tags)
            {
                var set = _dictionary.GetOrAdd(tag, x => new HashSet<string>());

                lock (set)
                {
                    set.Add(key);
                }
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetTaggedItemsAsync(string tag)
        {
            HashSet<string> set;
            if (_dictionary.TryGetValue(tag, out set))
            {
                lock (set)
                {
                    return Task.FromResult(set.AsEnumerable());
                }
            }

            return Task.FromResult(Enumerable.Empty<string>());
        }

        public Task RemoveTagAsync(string tag)
        {
            HashSet<string> set;

            if (_dictionary.TryRemove(tag, out set))
            {
                return _tagRemovedEventHandlers.InvokeAsync((handler, tag, set) => handler.TagRemovedAsync(tag, set), tag, set, _logger);
            }

            return Task.CompletedTask;
        }
    }
}
