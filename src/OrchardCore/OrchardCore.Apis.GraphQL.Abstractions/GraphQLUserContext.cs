using System;
using System.Security.Claims;

namespace OrchardCore.Apis.GraphQL
{
    public class GraphQLContext
    {
        public ClaimsPrincipal User { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
    }
}
