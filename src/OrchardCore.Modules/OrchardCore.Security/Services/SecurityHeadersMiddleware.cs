using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Services
{
    public class SecurityHeadersMiddleware
    {
        private readonly SecurityHeadersOptions _options;
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(IOptions<SecurityHeadersOptions> options, RequestDelegate next)
        {
            _options = options.Value;
            _next = next;
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
