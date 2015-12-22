using Orchard.Environment.Cache.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Cache
{
    public class CacheContextManager : ICacheContextManager
    {
        private readonly IEnumerable<ICacheContextProvider> _cacheContextProviders;

        public CacheContextManager(IEnumerable<ICacheContextProvider> cacheContextProviders)
        {
            _cacheContextProviders = cacheContextProviders;
        }

        public IEnumerable<CacheContextEntry> GetContext(IEnumerable<string> contexts)
        {
            var entries = new List<CacheContextEntry>();

            foreach (var provider in _cacheContextProviders.Reverse())
            {
                provider.PopulateContextEntries(contexts, entries);
            }

            return entries;
        }

    }
}
