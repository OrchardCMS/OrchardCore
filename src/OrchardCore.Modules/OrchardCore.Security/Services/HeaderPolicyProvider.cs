using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Services
{
    public abstract class HeaderPolicyProvider : IHeaderPolicyProvider
    {
        public SecurityHeadersOptions Options { get; set; }

        public abstract void InitPolicy();

        public abstract void ApplyPolicy(HttpContext httpContext);
    }
}
