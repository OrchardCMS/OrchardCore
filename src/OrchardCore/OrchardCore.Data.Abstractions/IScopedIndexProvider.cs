using YesSql.Indexes;

namespace OrchardCore.Data
{
    /// <summary>
    /// Marker interface to denote an <see cref="IIndexProvider"/> that needs to be resolved by the DI and registered
    /// at the <see cref="YesSql.ISession"/> level.
    /// </summary>
    public interface IScopedIndexProvider : IIndexProvider
    {
    }
}
