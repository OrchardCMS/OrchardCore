namespace OrchardCore.Environment.Cache.CacheContextProviders;

/// <summary>
/// Adds all context values as they are to the cache entries. This allows for known value variation.
/// </summary>
public class KnownValueCacheContextProvider : ICacheContextProvider
{
    public Task PopulateContextEntriesAsync(IEnumerable<string> contexts, List<CacheContextEntry> entries)
    {
        entries.Add(new CacheContextEntry("", string.Join(",", contexts)));

        return Task.CompletedTask;
    }
}
