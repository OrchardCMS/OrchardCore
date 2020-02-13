using YesSql;

namespace OrchardCore.Data
{
    /// <summary>
    /// A cacheable and committable document store using the <see cref="ISession"/>.
    /// </summary>
    public interface IDocumentStore : ICacheableDocumentStore, ICommittableDocumentStore
    {
    }
}
