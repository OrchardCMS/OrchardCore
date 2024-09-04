using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Services;

public class ContentSecurityPolicyHeaderPolicyProvider : HeaderPolicyProvider
{
    private string _policy;

    public override void InitializePolicy()
    {
        if (Options.ContentSecurityPolicy.Length > 0)
        {
            _policy = string.Join(SecurityHeaderDefaults.PoliciesSeparator, Options.ContentSecurityPolicy);
        }
    }

    public override void ApplyPolicy(HttpContext httpContext)
    {
        if (_policy != null)
        {
            httpContext.Response.Headers[SecurityHeaderNames.ContentSecurityPolicy] = _policy;
        }
    }
}
