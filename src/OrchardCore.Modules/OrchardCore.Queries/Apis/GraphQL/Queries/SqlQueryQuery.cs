using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.Queries.Sql;

namespace OrchardCore.Queries.Apis.GraphQL.Queries
{
    public class SqlQueryQuery : QueryFieldType
    {
        public SqlQueryQuery(
            IQueryManager queryManager) {

            var queries = queryManager.ListQueriesAsync();

            

            Name = "Query";

            Type = typeof(ListGraphType<ContentItemType2>);

            Arguments = new QueryArguments(
                new QueryArgument<StringGraphType> { Name = "Name" },
                new QueryArgument<StringGraphType> { Name = "Parameters" }
                );

            Resolver = new SlowFuncFieldResolver<object, Task<object>>(async (context) => {
                var name = context.GetArgument<string>("Name");

                var query = (await queryManager.GetQueryAsync(name)) as SqlQuery;

                var parameters = context.GetArgument<string>("Parameters");

                var queryParameters = parameters != null ?
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                    : new Dictionary<string, object>();

                var t = await queryManager.ExecuteQueryAsync(query, queryParameters);
                return t;
            });
        }
    }

    public class ContentItemType2 : AutoRegisteringObjectGraphType<ContentItem>
    {
        public ContentItemType2()
        {
            Name = "ContentItem";
        }
    }
}
