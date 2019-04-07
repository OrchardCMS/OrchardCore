using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Routing
{
    internal class ShellEndpointSelectorPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShellEndpointSelectorPolicy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override int Order => int.MinValue + 100;

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            var policies = _httpContextAccessor.HttpContext?.RequestServices
                .GetRequiredService<ShellMatcherPolicyProvider>()
                .GetEndpointSelectorPolicies()
                ?? Enumerable.Empty<IEndpointSelectorPolicy>();

            foreach (var policy in policies)
            {
                if (policy.AppliesToEndpoints(endpoints))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task ApplyAsync(HttpContext httpContext, EndpointSelectorContext context, CandidateSet candidates)
        {
            var policies = httpContext.RequestServices
                .GetRequiredService<ShellMatcherPolicyProvider>()
                .GetEndpointSelectorPolicies();

            foreach (var policy in policies)
            {
                await policy.ApplyAsync(httpContext, context, candidates);
            }
        }
    }
}