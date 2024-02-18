
using GraphQL.Transport;

namespace OrchardCore.Apis.GraphQL
{
    public class GraphQLNamedQueryRequest : GraphQLRequest
    {
        public string NamedQuery { get; set; }
    }
}
