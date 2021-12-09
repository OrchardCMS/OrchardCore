using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Queries
{
    /// <summary>
    /// Contract for query data source.
    /// </summary>
    public interface IQuerySource
    {
        /// <summary>
        /// Gets the name of query source.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Creates a query instance.
        /// </summary>
        Query Create();

        /// <summary>
        /// Executes a query with a given parameters.
        /// </summary>
        /// <param name="query">The <see cref="Query"/> to be executed.</param>
        /// <param name="parameters">The query parameters.</param>
        Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters);
    }
}
