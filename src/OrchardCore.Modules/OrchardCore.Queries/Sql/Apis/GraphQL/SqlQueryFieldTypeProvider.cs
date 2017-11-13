using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.Queries.Apis.GraphQL.Queries;

namespace OrchardCore.Queries.Sql.Apis.GraphQL.Queries
{
    public class SqlQueryFieldTypeProvider : IDynamicQueryFieldTypeProvider
    {
        private readonly IQueryManager _queryManager;
        private readonly IEnumerable<QueryFieldType> _queryFieldTypes;
        private readonly IDependencyResolver _dependencyResolver;

        public SqlQueryFieldTypeProvider(IQueryManager queryManager,
            IEnumerable<QueryFieldType> queryFieldTypes,
            IDependencyResolver dependencyResolver)
        {
            _queryManager = queryManager;
            _queryFieldTypes = queryFieldTypes;
            _dependencyResolver = dependencyResolver;
        }

        public async Task<IEnumerable<FieldType>> GetFields(ObjectGraphType state)
        {
            var queries = await _queryManager.ListQueriesAsync();

            var queryType = new ObjectGraphType { Name = "Query" };

            foreach (var query in queries.OfType<SqlQuery>())
            {
                var name = query.Name;
                var source = query.Source;

                var graphType = new SqlQueryQuery(query, _dependencyResolver);

                var fieldType = new FieldType
                {
                    Arguments = new QueryArguments(
                        new QueryArgument<StringGraphType> { Name = "Name" },
                        new QueryArgument<StringGraphType> { Name = "Parameters" }
                    ),
                    
                    Name = name,
                    ResolvedType = graphType,
                    Resolver = new SlowFuncFieldResolver<object, Task<object>>(async context => {
                        var iname = context.GetArgument<string>("Name");

                        var iquery = await _queryManager.GetQueryAsync(iname);

                        var parameters = context.GetArgument<string>("Parameters");

                        var queryParameters = parameters != null ?
                            JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                            : new Dictionary<string, object>();

                        return await _queryManager.ExecuteQueryAsync(iquery, queryParameters);
                    }),
                    Type = graphType.GetType(),
                };

                queryType.AddField(fieldType);

            }

            return new FieldType[] { new QueriesQuery<SqlQueryQuery> {
                Name = "SqlQuery",
                ResolvedType = queryType,
                Type = typeof(SqlQueryQuery)
            } };
        }
    }
}
