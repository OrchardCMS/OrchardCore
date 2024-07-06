using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Queries.Core;

public interface IQueryManager
{
    /// <summary>
    /// Saves the specific <see cref="Query"/>.
    /// </summary>
    /// <param name="queries">The <see cref="Query"/> instance to save.</param>
    Task SaveQueryAsync(params Query[] queries);

    /// <summary>
    /// Deletes the specified <see cref="Query"/>.
    /// </summary>
    /// <param name="name">The name of the query to delete.</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteQueryAsync(params string[] name);

    /// <summary>
    /// Gets the <see cref="Query"/> instance with the specified name in read-only.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>The found query if any, otherwise null.</returns>
    Task<Query> GetQueryAsync(string name);

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="parameters">The parameters for the query.</param>
    /// <returns>The result of the query.</returns>
    Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters);

    /// <summary>
    /// Returns an unique identifier that is updated when queries have changed.
    /// </summary>
    Task<string> GetIdentifierAsync();

    Task<IEnumerable<Query>> ListQueriesAsync(bool sorted = false);

    Task<IEnumerable<Query>> ListQueriesBySourceAsync(string sourceName, bool sorted = false);
}
