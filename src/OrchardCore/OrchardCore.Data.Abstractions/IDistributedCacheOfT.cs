namespace OrchardCore.Data
{
    /// <summary>
    /// A generic service to keep in sync a multi level distributed cache with a given document store.
    /// </summary>
    public interface IDistributedCache<TDocumentStore> : IDocumentStoreDistributedCache where TDocumentStore : ICacheableDocumentStore
    {
    }
}
