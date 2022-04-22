using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Services
{
    public interface IHeaderPolicyProvider
    {
        void ApplyPolicy(HttpContext httpContext);
    }
}
