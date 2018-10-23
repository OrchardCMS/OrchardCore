using System.Security.Claims;

namespace OrchardCore.Apis.GraphQL
{
    public class GraphQLUserContext
    {
        public ClaimsPrincipal User { get; set; }
    }
}
