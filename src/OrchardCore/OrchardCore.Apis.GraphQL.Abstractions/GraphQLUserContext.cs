using System;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Apis.GraphQL
{
    public class GraphQLContext
    {
        public HttpContext HttpContext { get; set; }
        public ClaimsPrincipal User { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public SemaphoreSlim ExecutionContextLock { get; } = new SemaphoreSlim(1, 1);
    }
}
