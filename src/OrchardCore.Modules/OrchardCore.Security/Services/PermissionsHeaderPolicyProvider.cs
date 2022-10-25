using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Services
{
    public class PermissionsHeaderPolicyProvider : HeaderPolicyProvider
    {
        private string _policy;

        public override void InitializePolicy()
        {
            if (Options.PermissionsPolicy.Length > 0)
            {
                _policy = String.Join(SecurityHeaderDefaults.PoliciesSeparator, Options.PermissionsPolicy);
            }
        }

        public override void ApplyPolicy(HttpContext httpContext)
        {
            if (_policy != null)
            {
                httpContext.Response.Headers[SecurityHeaderNames.PermissionsPolicy] = _policy;
            }
        }
    }
}
