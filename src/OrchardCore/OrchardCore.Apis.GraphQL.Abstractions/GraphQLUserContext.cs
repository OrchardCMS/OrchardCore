using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Apis.GraphQL
{
    public class GraphQLContext
    {
        public HttpContext HttpContext { get; set; }
        public ClaimsPrincipal User { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
    }
}
