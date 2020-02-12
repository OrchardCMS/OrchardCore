using YesSql;

namespace OrchardCore.Data
{
    /// <summary>
    /// Document store implementation using the <see cref="ISession"/>.
    /// </summary>
    public interface ISessionDocumentStore : ICacheableDocumentStore, ICommittableDocumentStore
    {
    }
}
