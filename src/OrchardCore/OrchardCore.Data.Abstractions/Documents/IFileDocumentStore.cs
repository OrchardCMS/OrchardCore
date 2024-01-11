namespace OrchardCore.Data.Documents
{
    /// <summary>
    /// A singleton service using the file system to store document files under the tenant folder, and that is in sync
    /// with the ambient transaction, any file is updated after a successful <see cref="IDocumentStore.CommitAsync"/>.
    /// </summary>
    public interface IFileDocumentStore : IDocumentStore
    {
    }
}
