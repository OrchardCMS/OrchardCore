
using GraphQL.Transport;
using OrchardCore.Apis.GraphQL.Queries;

namespace OrchardCore.Apis.GraphQL;

public class GraphQLNamedQueryRequest : GraphQLRequest
{
    /// <summary>
    /// Used to store some graphql query on the server, and then the client only needs to submit the name of that query to reduce the size of the network request
    /// <see cref="INamedQueryProvider"/>
    /// </summary>
    public string NamedQuery { get; set; }
}
