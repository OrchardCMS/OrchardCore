using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Services
{
    public interface IHeaderPolicyProvider
    {
        void InitPolicy();

        void ApplyPolicy(HttpContext httpContext);
    }
}
