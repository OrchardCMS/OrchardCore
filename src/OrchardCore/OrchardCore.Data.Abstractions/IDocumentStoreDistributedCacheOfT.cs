namespace OrchardCore.Data
{
    /// <summary>
    /// A generic service to keep in sync a given document store with a multi level distributed cache.
    /// </summary>
    public interface IDocumentStoreDistributedCache<TDocumentStore> : IDocumentStoreDistributedCache where TDocumentStore : ICacheableDocumentStore
    {
    }
}
