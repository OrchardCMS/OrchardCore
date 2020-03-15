namespace OrchardCore.Data
{
    /// <summary>
    /// A cacheable and committable document store.
    /// </summary>
    public interface IDocumentStore : ICacheableDocumentStore, ICommittableDocumentStore
    {
    }
}
