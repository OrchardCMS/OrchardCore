using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Apis.GraphQL
{
    public class GraphQLSettings
    {
        public PathString Path { get; set; } = "/api/graphql";
        public Func<HttpContext, object> BuildUserContext { get; set; }
        public bool ExposeExceptions = false;
    }
}