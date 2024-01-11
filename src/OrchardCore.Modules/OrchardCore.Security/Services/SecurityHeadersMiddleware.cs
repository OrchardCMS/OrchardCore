using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Services
{
    public class SecurityHeadersMiddleware
    {
        private readonly SecurityHeadersOptions _options;
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(SecurityHeadersOptions options, RequestDelegate next)
        {
            _options = options;
            _next = next;

            foreach (var provider in _options.HeaderPolicyProviders)
            {
                provider.InitializePolicy();
            }
        }

        public Task Invoke(HttpContext context)
        {
            foreach (var provider in _options.HeaderPolicyProviders)
            {
                provider.ApplyPolicy(context);
            }

            return _next.Invoke(context);
        }
    }
}
