using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Services
{
    public class PermissionsHeaderPolicyProvider : HeaderPolicyProvider
    {
        private static readonly string _separator = ", ";

        public override void ApplyPolicy(HttpContext httpContext)
        {
            httpContext.Response.Headers[SecurityHeaderNames.PermissionsPolicy] = String.Join(_separator, Options.PermissionsPolicy);
        }
    }
}
