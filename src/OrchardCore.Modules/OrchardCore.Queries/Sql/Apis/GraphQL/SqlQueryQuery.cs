using System.Linq;
using GraphQL;
using GraphQL.Types;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Queries.Sql.Apis.GraphQL.Queries
{

    public class SqlQueryQuery : ObjectGraphType<JObject>
    {
        public SqlQueryQuery(SqlQuery query,
            IDependencyResolver dependencyResolver)
        {
            Name = query.Name;

            var schemaJson = JObject.Parse(@"{
  ""schema"": {
	""Title"": ""String"",
	""Id"": ""Integer""
  }
}");

            var schema = schemaJson["schema"];

            foreach (var child in schema.Children().OfType<JProperty>())
            {
                var name = child.Name.ToString();
                var value = child.Value.ToString();

                if (value == "String")
                {
                    Field(
                        typeof(StringGraphType), 
                        name,
                        resolve: context => {
                            var source = context.Source;
                            return ((JProperty)source[context.FieldName]).ToObject<string>();
                        });
                }
                if (value == "Integer")
                {
                    Field(
                        typeof(IntGraphType), 
                        name,
                        resolve: context => {
                            var source = context.Source;
                            return ((JProperty)source[context.FieldName]).ToObject<int>();
                        });
                }
            }
        }
    }
}
