using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Services
{
    public class ContentSecurityPolicyHeaderPolicyProvider : HeaderPolicyProvider
    {
        public override void ApplyPolicy(HttpContext httpContext)
        {
            if (!Options.ContentSecurityPolicy.Equals(SecurityHeaderDefaults.ContentSecurityPolicy))
            {
                httpContext.Response.Headers[SecurityHeaderNames.ContentSecurityPolicy] = String.Join(SecurityHeaderDefaults.PoliciesSeparater, Options.ContentSecurityPolicy);
            }
        }
    }
}
