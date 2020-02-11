using YesSql;

namespace OrchardCore.Data
{
    /// <summary>
    /// Data store implementation using the <see cref="ISession"/>.
    /// </summary>
    public interface ISessionDataStore : ICacheableDataStore, ICommittableDataStore
    {
    }
}
