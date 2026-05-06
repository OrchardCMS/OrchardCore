using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting;

namespace OrchardCore.Queries;

public sealed class QueryGlobalMethodProvider : IGlobalMethodProvider
{
    private readonly GlobalMethod _executeQuery;

    /// <summary>
    /// Usage: executeQuery(name, parameters)
    /// Ex: executeQuery("MySqlQuery", {"Owner":"bob"});.
    /// </summary>
    public QueryGlobalMethodProvider()
    {
        _executeQuery = new GlobalMethod
        {
            Name = "executeQuery",
            Method = serviceProvider => (Func<string, object, object>)((name, parameters) =>
                ExecuteQueryAsync(serviceProvider, name, parameters).GetAwaiter().GetResult()),
            AsyncMethod = serviceProvider => (Func<string, object, Task<object>>)((name, parameters) =>
                ExecuteQueryAsync(serviceProvider, name, parameters)),
        };
    }

    public IEnumerable<GlobalMethod> GetMethods()
    {
        return [_executeQuery];
    }

    private static async Task<object> ExecuteQueryAsync(IServiceProvider serviceProvider, string name, object parameters)
    {
        var queryManager = serviceProvider.GetRequiredService<IQueryManager>();
        var query = await queryManager.GetQueryAsync(name);

        if (query == null)
        {
            return null;
        }

        var result = await queryManager.ExecuteQueryAsync(query, (IDictionary<string, object>)parameters);

        return result.Items;
    }
}
