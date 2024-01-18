using System.Text.Json;

namespace OrchardCore.Apis.GraphQL
{
    public class GraphQLRequest
    {
        public string OperationName { get; set; }
        public string NamedQuery { get; set; }
        public string Query { get; set; }

        public JsonElement Variables { get; set; }
    }
}
