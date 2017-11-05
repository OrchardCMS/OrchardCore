using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Queries.Apis.GraphQL.Queries
{
    public class QueryQuery : QueryFieldType
    {
        public QueryQuery(
            IQueryManager queryManager) {

            Name = "Query";

            //Type = typeof(ContentItemType);

            Arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "Name" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "Parameters" }
                );

            Resolver = new SlowFuncFieldResolver<object, Task<object>>(async (context) => {
                var name = context.GetArgument<string>("Name");

                var parameters = JsonConvert.DeserializeObject<IDictionary<string, object>>(
                    context.GetArgument<string>("Parameters"));

                var query = await queryManager.GetQueryAsync(name);

                return await queryManager.ExecuteQueryAsync(query, parameters);
            });
        }
    }
}
