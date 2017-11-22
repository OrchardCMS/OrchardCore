using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Queries
{
    public interface IQueryManager
    {
        /// <summary>
        /// Returns a list of all store <see cref="Query"/>.
        /// </summary>
        Task<IEnumerable<Query>> ListQueriesAsync();

        /// <summary>
        /// Saves the specific <see cref="Query"/>.
        /// </summary>
        /// <param name="name">The name of the query to update.</param>
        /// <param name="query">The <see cref="Query"/> instance to save.</param>
        Task SaveQueryAsync(string name, Query query);

        /// <summary>
        /// Deletes the specified <see cref="Query"/>.
        /// </summary>
        /// <param name="name">The name of the query to delete.</param>
        Task DeleteQueryAsync(string name);

        /// <summary>
        /// Gets the <see cref="Query"/> instance with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<Query> GetQueryAsync(string name);

        /// <summary>
        /// Executes a query.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <returns>The result of the query.</returns>
        Task<object> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters);
    }
}
