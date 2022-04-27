using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Services
{
    public class PermissionsHeaderPolicyProvider : HeaderPolicyProvider
    {
        public override void ApplyPolicy(HttpContext httpContext)
        {
            httpContext.Response.Headers[SecurityHeaderNames.PermissionsPolicy] = String.Join(SecurityHeaderDefaults.PoliciesSeparater, Options.PermissionsPolicy);
        }
    }
}
