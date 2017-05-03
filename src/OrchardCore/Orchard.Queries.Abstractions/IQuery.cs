
namespace Orchard.Queries
{
    /// <summary>
    /// Classes implementing <see cref="IQuery"/> represent a serializable query for an <see cref="IQuerySource"/>.
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// Gets the technical name of the query.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the name of the source for this query.
        /// </summary>
        string Source { get; }
    }
}
