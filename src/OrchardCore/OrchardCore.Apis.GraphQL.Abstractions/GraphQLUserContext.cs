using System.Security.Claims;

namespace OrchardCore.Apis.GraphQL;

public class GraphQLUserContext : Dictionary<string, object>
{
    public ClaimsPrincipal User { get; set; }
    public SemaphoreSlim ExecutionContextLock { get; } = new SemaphoreSlim(1, 1);
}
