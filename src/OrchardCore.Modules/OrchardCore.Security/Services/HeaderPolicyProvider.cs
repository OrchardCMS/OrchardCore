using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Services
{
    public abstract class HeaderPolicyProvider : IHeaderPolicyProvider
    {
        public SecurityHeadersOptions Options { get; set; }

        public virtual void InitializePolicy()
        {
            // This is intentionally empty, only header policy provider(s) that need an initialization should override this
        }

        public abstract void ApplyPolicy(HttpContext httpContext);
    }
}
