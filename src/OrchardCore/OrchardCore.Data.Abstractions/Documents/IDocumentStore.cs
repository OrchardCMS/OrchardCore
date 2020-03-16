namespace OrchardCore.Data.Documents
{
    /// <summary>
    /// A cacheable and committable document store.
    /// </summary>
    public interface IDocumentStore : ICacheableDocumentStore, ICommittableDocumentStore
    {
    }
}
