using Newtonsoft.Json.Linq;

namespace OrchardCore.Apis.GraphQL
{
    public class GraphQLRequest
    {
        public string OperationName { get; set; }
        public string NamedQuery { get; set; }
        public string Query { get; set; }
        public JObject Variables { get; set; }
    }
}
