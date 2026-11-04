namespace OrchardCore.Environment.Cache;

public class CacheContextManager : ICacheContextManager
{
    private readonly ICacheContextProvider[] _cacheContextProviders;

    public CacheContextManager(IEnumerable<ICacheContextProvider> cacheContextProviders)
    {
        // The providers resolved from the container are already an array, it is iterated without copying it.
        _cacheContextProviders = cacheContextProviders as ICacheContextProvider[] ?? cacheContextProviders.ToArray();
    }

    public async Task<IEnumerable<CacheContextEntry>> GetDiscriminatorsAsync(IEnumerable<string> contexts)
    {
        var entries = new List<CacheContextEntry>();

        // The providers are invoked in the reverse order of their registration.
        for (var i = _cacheContextProviders.Length - 1; i >= 0; i--)
        {
            await _cacheContextProviders[i].PopulateContextEntriesAsync(contexts, entries);
        }

        return entries;
    }
}
