using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Queries;

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
        var queryManager = orchardHelper.HttpContext.RequestServices.GetService<IQueryManager>();

        var query = await queryManager.GetQueryAsync(queryName);

        if (query == null)
        {
            return null;
        }

        var result = await queryManager.ExecuteQueryAsync(query, parameters);
        return result.Items;
    }

    public static async Task<IQueryResults> QueryResultsAsync(this IOrchardHelper orchardHelper, string queryName, IDictionary<string, object> parameters)
    {
        var queryManager = orchardHelper.HttpContext.RequestServices.GetService<IQueryManager>();

        var query = await queryManager.GetQueryAsync(queryName);

        if (query == null)
        {
            return null;
        }

        var result = await queryManager.ExecuteQueryAsync(query, parameters);
        return result;
    }
}
