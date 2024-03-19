
using GraphQL.Transport;

namespace OrchardCore.Apis.GraphQL
{
    public class GraphQLNamedQueryRequest : GraphQLRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public string NamedQuery { get; set; }
    }
}
