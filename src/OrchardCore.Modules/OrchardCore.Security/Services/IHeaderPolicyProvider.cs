using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Services
{
    public interface IHeaderPolicyProvider
    {
        void InitializePolicy();

        void ApplyPolicy(HttpContext httpContext);
    }
}
