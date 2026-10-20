namespace OrchardCore.Environment.Cache;

public class CacheContextManager : ICacheContextManager
{
    private readonly IEnumerable<ICacheContextProvider> _cacheContextProviders;
    private ICacheContextProvider[] _reversedProviders;

    public CacheContextManager(IEnumerable<ICacheContextProvider> cacheContextProviders)
    {
        _cacheContextProviders = cacheContextProviders;
    }

    public async Task<IEnumerable<CacheContextEntry>> GetDiscriminatorsAsync(IEnumerable<string> contexts)
    {
        var entries = new List<CacheContextEntry>();

        // The providers are invoked in the reverse order of their registration, the array is only built once
        // per scope instead of a reversed copy for each cached shape.
        if (_reversedProviders is null)
        {
            var providers = _cacheContextProviders.ToArray();
            Array.Reverse(providers);
            _reversedProviders = providers;
        }

        foreach (var provider in _reversedProviders)
        {
            await provider.PopulateContextEntriesAsync(contexts, entries);
        }

        return entries;
    }
}
