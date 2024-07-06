using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Queries.Core;

public static class QueryManagerExtensions
{
    public static Task<IEnumerable<Query>> ListQueriesBySourceAsync(this IQueryManager queryManager, string sourceName)
    {
        return queryManager.ListQueriesAsync(new QueryContext()
        {
            Source = sourceName,
        });
    }

    public static Task<IEnumerable<Query>> ListQueriesAsync(this IQueryManager queryManager, bool sorted)
    {
        return queryManager.ListQueriesAsync(new QueryContext()
        {
            Sorted = sorted,
        });
    }
}
