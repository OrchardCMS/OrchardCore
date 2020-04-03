using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting;

namespace OrchardCore.Queries
{
    public class QueryGlobalMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _executeQuery;

        /// <summary>
        /// Usage: executeQuery(name, parameters)
        /// Ex: executeQuery("MySqlQuery", {"Owner":"bob"});
        /// </summary>
        public QueryGlobalMethodProvider()
        {
            _executeQuery = new GlobalMethod
            {
                Name = "executeQuery",
                Method = serviceProvider => (Func<string, object, object>)((name, parameters) =>
                {
                    var queryManager = serviceProvider.GetRequiredService<IQueryManager>();
                    var query = queryManager.GetQueryAsync(name).GetAwaiter().GetResult();
                    if (query == null)
                    {
                        return null;
                    }
                    var result = queryManager.ExecuteQueryAsync(query, (IDictionary<string, object>)parameters).GetAwaiter().GetResult();
                    return result;
                }
                  )
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _executeQuery };
        }
    }
}
