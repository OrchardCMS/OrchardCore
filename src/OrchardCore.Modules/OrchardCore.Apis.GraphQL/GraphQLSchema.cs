using System;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL
{
    public class GraphQLSchema : Schema
    {
        public GraphQLSchema(IServiceProvider provider)
            : base(provider)
        {
        }
    }
}
