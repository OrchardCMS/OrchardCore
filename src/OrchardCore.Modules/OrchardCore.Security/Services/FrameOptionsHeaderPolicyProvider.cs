using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Services
{
    public class FrameOptionsHeaderPolicyProvider : HeaderPolicyProvider
    {
        public override void ApplyPolicy(HttpContext httpContext)
        {
            var overrideHeaderByCSP = false;
            if (httpContext.Response.Headers.ContainsKey(SecurityHeaderNames.ContentSecurityPolicy))
            {
                var contentSecurityHeaderValue = httpContext.Response.Headers[SecurityHeaderNames.ContentSecurityPolicy].ToString();
                // Override FramOptions when FrameAncestors exists
                if (contentSecurityHeaderValue.Contains(ContentSecurityPolicyValue.FrameAncestors, System.StringComparison.CurrentCulture))
                {
                    overrideHeaderByCSP = true;
                }
            }

            if (!overrideHeaderByCSP)
            {
                httpContext.Response.Headers[SecurityHeaderNames.XFrameOptions] = Options.FrameOptions;
            }
        }
    }
}
