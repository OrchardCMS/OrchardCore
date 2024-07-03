using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Queries.Indexes;
using OrchardCore.Scripting;
using YesSql;

namespace OrchardCore.Queries
{
    public class QueryGlobalMethodProvider : IGlobalMethodProvider
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
                {
                    var session = serviceProvider.GetRequiredService<ISession>();
                    var query = session.Query<Query, QueryIndex>(q => q.Name == name).FirstOrDefaultAsync().GetAwaiter().GetResult();

                    if (query == null)
                    {
                        return null;
                    }

                    var querySource = serviceProvider.GetRequiredKeyedService<IQuerySource>(query.Source);

                    var result = querySource.ExecuteQueryAsync(query, (IDictionary<string, object>)parameters).GetAwaiter().GetResult();

                    return result.Items;
                }),
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return [_executeQuery];
        }
    }
}
