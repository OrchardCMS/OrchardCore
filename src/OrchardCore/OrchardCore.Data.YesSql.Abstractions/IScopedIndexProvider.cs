using YesSql;
using YesSql.Indexes;

namespace OrchardCore.Data
{
    /// <summary>
    /// Represents a contract that used to denote an <see cref="IIndexProvider"/> that needs to be resolved by the DI and registered
    /// at the <see cref="ISession"/> level.
    /// </summary>
    public interface IScopedIndexProvider : IIndexProvider
    {
    }
}
