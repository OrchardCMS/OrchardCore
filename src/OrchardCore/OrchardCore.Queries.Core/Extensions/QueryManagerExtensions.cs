namespace OrchardCore.Queries;

public static class QueryManagerExtensions
{
    public static Task<IEnumerable<Query>> ListQueriesBySourceAsync(this IQueryManager queryManager, string sourceName)
        => queryManager.ListQueriesAsync(new QueryContext()
        {
            Source = sourceName,
        });

    public static Task<IEnumerable<Query>> ListQueriesAsync(this IQueryManager queryManager, bool sorted)
        => queryManager.ListQueriesAsync(new QueryContext()
        {
            Sorted = sorted,
        });
}
