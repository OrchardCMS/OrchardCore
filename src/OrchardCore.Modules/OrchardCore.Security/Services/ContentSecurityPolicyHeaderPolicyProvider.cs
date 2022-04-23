using System;
using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Services
{
    public class ContentSecurityPolicyHeaderPolicyProvider : HeaderPolicyProvider
    {
        public override void ApplyPolicy(HttpContext httpContext)
        {
            httpContext.Response.Headers[SecurityHeaderNames.ContentSecurityPolicy] = String.Join(ContentSecurityPolicyOptions.Separator, Options.ContentSecurityPolicy);
        }
    }
}
