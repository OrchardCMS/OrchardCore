using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Services
{
    public class ReferrerHeaderPolicyProvider : HeaderPolicyProvider
    {
        public override void ApplyPolicy(HttpContext httpContext)
        {
            httpContext.Response.Headers[SecurityHeaderNames.ReferrerPolicy] = Options.ReferrerPolicy;
        }
    }
}
