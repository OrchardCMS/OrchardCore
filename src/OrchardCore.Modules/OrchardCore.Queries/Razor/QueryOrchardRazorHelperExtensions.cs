using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Queries;

public static class QueryOrchardRazorHelperExtensions
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
}
