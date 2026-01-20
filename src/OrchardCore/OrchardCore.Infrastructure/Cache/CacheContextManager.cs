using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Cache
{
    public class CacheContextManager : ICacheContextManager
    {
        private readonly IEnumerable<ICacheContextProvider> _cacheContextProviders;

        public CacheContextManager(IEnumerable<ICacheContextProvider> cacheContextProviders)
        {
            _cacheContextProviders = cacheContextProviders;
        }

        public async Task<IEnumerable<CacheContextEntry>> GetDiscriminatorsAsync(IEnumerable<string> contexts)
        {
            var entries = new List<CacheContextEntry>();

            foreach (var provider in _cacheContextProviders.Reverse())
            {
                await provider.PopulateContextEntriesAsync(contexts, entries);
            }

            return entries;
        }
    }
}
