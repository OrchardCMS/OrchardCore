using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Services
{
    public class PermissionsHeaderPolicyProvider : HeaderPolicyProvider
    {
        public override void ApplyPolicy(HttpContext httpContext)
        {
            if (Options.PermissionsPolicy.Length > 0)
            {
                httpContext.Response.Headers[SecurityHeaderNames.PermissionsPolicy] = String.Join(SecurityHeaderDefaults.PoliciesSeparator, Options.PermissionsPolicy);
            }
        }
    }
}
