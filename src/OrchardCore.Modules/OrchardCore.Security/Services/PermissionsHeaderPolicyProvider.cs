using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Services
{
    public class PermissionsHeaderPolicyProvider : HeaderPolicyProvider
    {
        public override void ApplyPolicy(HttpContext httpContext)
        {
            if (Options.PermissionsPolicy.Count > 0)
            {
                var policies = Options.PermissionsPolicy.Select(p => $"{p.Key}={p.Value}");

                httpContext.Response.Headers[SecurityHeaderNames.PermissionsPolicy] = String.Join(SecurityHeaderDefaults.PoliciesSeparator, policies);
            }
        }
    }
}
