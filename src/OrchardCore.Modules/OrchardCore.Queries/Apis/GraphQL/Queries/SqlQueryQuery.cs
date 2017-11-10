using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Queries.Apis.GraphQL.Queries
{
    public class QueryTypeFieldTypeProvider : IDynamicQueryFieldTypeProvider
    {
        private readonly IQueryManager _queryManager;
        private readonly IEnumerable<QueryFieldType> _queryFieldTypes;
        private readonly IDependencyResolver _dependencyResolver;

        public QueryTypeFieldTypeProvider(IQueryManager queryManager,
            IEnumerable<QueryFieldType> queryFieldTypes,
            IDependencyResolver dependencyResolver)
        {
            _queryManager = queryManager;
            _queryFieldTypes = queryFieldTypes;
            _dependencyResolver = dependencyResolver;
        }

        public async Task<IEnumerable<FieldType>> GetFields()
        {
            var queries = await _queryManager.ListQueriesAsync();

            var queryType = new QueriesQueryType();

            foreach (var query in queries)
            {
                var name = query.Name;
                var source = query.Source;

                var graphType = new DynamicQueriesQuery(query, _dependencyResolver);

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

            return new FieldType[] { new QueriesQuery {
                Name = "Query",
                ResolvedType = queryType
            } };
        }
    }

    public class QueriesQueryType : ObjectGraphType
    {
        public QueriesQueryType()
        {
            Name = "Query";
        }
    }

    public class QueriesQuery : QueryFieldType { }

    public class DynamicQueriesQuery : ObjectGraphType
    {
        public DynamicQueriesQuery(Query query,
            IDependencyResolver dependencyResolver)
        {
            Name = query.Name;

            var schema = JObject.Parse(query.Schema);

            foreach (var child in schema.Properties())
            {
                AddField(new FieldType
                {
                    Name = child.Name,

                    Type = child..Name.Value..Type.GetGraphTypeFromType(child. ().GetType().IsNullable())
                });
            }
        }
    }
}
