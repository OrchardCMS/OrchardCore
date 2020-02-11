namespace OrchardCore.Data
{
    /// <summary>
    /// A generic scoped service to keep in sync a given data store with a distributed cache.
    /// </summary>
    public interface IDataStoreDistributedCache<TDataStore> : IDataStoreDistributedCache where TDataStore : ICacheableDataStore
    {
    }
}
