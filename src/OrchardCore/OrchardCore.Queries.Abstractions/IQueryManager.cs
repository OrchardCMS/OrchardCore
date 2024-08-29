using System.Text.Json.Nodes;

namespace OrchardCore.Queries;

public interface IQueryManager
{
    /// <summary>
    /// Gets a new instance of <see cref="Query"/> for the given source if a valid source is given.
    /// </summary>
    /// <returns>A new instance of <see cref="Query"/>.</returns>
    Task<Query> NewAsync(string source, JsonNode data = null);

    /// <summary>
    /// Updated the Query using the given data and saves it into the store.
    /// </summary>
    Task UpdateAsync(Query query, JsonNode data = null);

    /// <summary>
    /// Deletes the specified <see cref="Query"/>.
    /// </summary>
    /// <param name="name">The name of the query to delete.</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteQueryAsync(params string[] name);

    /// <summary>
    /// Gets the <see cref="Query"/> instance with the specified name in read-only.
    /// </summary>
    /// <param name="name">The name of the query.</param>
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
    /// <returns>A unique string identifier represent all the queries.</returns>
    Task<string> GetIdentifierAsync();

    /// <summary>
    /// Gets a list of stored <see cref="Query"/> that match the given context.
    /// </summary>
    /// <param name="context">The context provides a way to filter the returned dataset.</param>
    /// <returns>IEnumerable of <see cref="Query"/> type.</returns>
    Task<IEnumerable<Query>> ListQueriesAsync(QueryContext context = null);

    /// <summary>
    /// Gets a paged list of stored <see cref="Query"/> that match the given context.
    /// </summary>
    /// <param name="page">The page number to return.</param>
    /// <param name="pageSize">The page size to return.</param>
    /// <param name="context">The context provides a way to filter the returned dataset.</param>
    /// <returns>An instance of <see cref="ListQueryResult"/>.</returns>
    Task<ListQueryResult> PageQueriesAsync(int page, int pageSize, QueryContext context = null);

    /// <summary>
    /// Saves the given queries to the store.
    /// </summary>
    /// <param name="queries">One or more instances of <see cref="Query"/> to save.</param>
    Task SaveAsync(params Query[] queries);
}
