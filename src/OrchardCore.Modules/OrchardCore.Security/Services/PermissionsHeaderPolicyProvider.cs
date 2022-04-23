using System;
using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Services
{
    public class PermissionsHeaderPolicyProvider : HeaderPolicyProvider
    {
        public override void ApplyPolicy(HttpContext httpContext)
        {
            httpContext.Response.Headers[SecurityHeaderNames.PermissionsPolicy] = String.Join(PermissionsPolicyOptions.Separator, Options.PermissionsPolicy);
        }
    }
}
