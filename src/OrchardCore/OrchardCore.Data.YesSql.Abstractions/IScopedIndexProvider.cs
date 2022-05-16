using YesSql.Indexes;

namespace OrchardCore.Data
{

#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
    /// <summary>
    /// Represents a contract that used to denote an <see cref="IIndexProvider"/> that needs to be resolved by the DI and registered
    /// at the <see cref="YesSql.ISession"/> level.
    /// </summary>
    public interface IScopedIndexProvider : IIndexProvider
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
    {
    }
}
