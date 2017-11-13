//using System.Linq;
//using GraphQL;
//using GraphQL.Types;
//using Newtonsoft.Json.Linq;
//using OrchardCore.Lucene;

//namespace OrchardCore.Queries.Lucene.Apis.GraphQL.Queries
//{

//    public class LuceneQueryQuery : ObjectGraphType<JObject>
//    {
//        public LuceneQueryQuery(LuceneQuery query,
//            IDependencyResolver dependencyResolver)
//        {
//            Name = query.Name;

//            if (query.ReturnContentItems)
//            {
//                // Return ContentType... need to infur from contents module

//            }

//            var schemaJson = JObject.Parse(@"{
//  ""schema"": {
//	""Title"": ""String"",
//	""Id"": ""Integer""
//  }
//}");

//            var schema = schemaJson["schema"];

//            foreach (var child in schema.Children().OfType<JProperty>())
//            {
//                var name = child.Name.ToString();
//                var value = child.Value.ToString();

//                if (value == "String")
//                {
//                    Field(
//                        typeof(StringGraphType), 
//                        name,
//                        resolve: context => {
//                            var source = context.Source;
//                            return ((JProperty)source[context.FieldName]).ToObject<string>();
//                        });
//                }
//                if (value == "Integer")
//                {
//                    Field(
//                        typeof(IntGraphType), 
//                        name,
//                        resolve: context => {
//                            var source = context.Source;
//                            return ((JProperty)source[context.FieldName]).ToObject<int>();
//                        });
//                }
//            }
//        }
//    }
//}
