using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Services
{
    public class FrameOptionsHeaderPolicyProvider : HeaderPolicyProvider
    {
        public override void ApplyPolicy(HttpContext httpContext)
        {
            httpContext.Response.Headers[SecurityHeaderNames.XFrameOptions] = Options.FrameOptions;
        }
    }
}
