using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Queries;
using OrchardCore.Queries.Indexes;
using YesSql;

#pragma warning disable CA1050 // Declare types in namespaces
public static class QueryOrchardRazorHelperExtensions
#pragma warning restore CA1050 // Declare types in namespaces
{
    public static Task<IEnumerable> QueryAsync(this IOrchardHelper orchardHelper, string queryName)
    {
        return QueryAsync(orchardHelper, queryName, new Dictionary<string, object>());
    }

    public static async Task<IEnumerable> QueryAsync(this IOrchardHelper orchardHelper, string queryName, IDictionary<string, object> parameters)
    {
        var session = orchardHelper.HttpContext.RequestServices.GetService<ISession>();

        var query = await session.Query<Query, QueryIndex>(q => q.Name == queryName).FirstOrDefaultAsync();

        if (query == null)
        {
            return null;
        }

        var querySource = orchardHelper.HttpContext.RequestServices.GetRequiredKeyedService<IQuerySource>(query.Source);

        var result = await querySource.ExecuteQueryAsync(query, parameters);

        return result.Items;
    }

    public static async Task<IQueryResults> QueryResultsAsync(this IOrchardHelper orchardHelper, string queryName, IDictionary<string, object> parameters)
    {
        var session = orchardHelper.HttpContext.RequestServices.GetService<ISession>();

        var query = await session.Query<Query, QueryIndex>(q => q.Name == queryName).FirstOrDefaultAsync();

        if (query == null)
        {
            return null;
        }

        var querySource = orchardHelper.HttpContext.RequestServices.GetRequiredKeyedService<IQuerySource>(query.Source);

        var result = await querySource.ExecuteQueryAsync(query, parameters);

        return result;
    }
}
