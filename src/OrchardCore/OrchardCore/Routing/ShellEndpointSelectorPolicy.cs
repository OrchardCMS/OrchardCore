using System;
using System.Collections.Generic;
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

            var matchers = _httpContextAccessor.HttpContext?.RequestServices
                .GetRequiredService<ShellMatcherPolicyProvider>()
                .GetPolicies();

            foreach (var matcher in matchers)
            {
                if (matcher is IEndpointSelectorPolicy policy && policy.AppliesToEndpoints(endpoints))
                {
                    return true;
                }
            }

            return false;
        }

        public Task ApplyAsync(HttpContext httpContext, EndpointSelectorContext context, CandidateSet candidates)
        {
            var matchers = _httpContextAccessor.HttpContext?.RequestServices
                .GetRequiredService<ShellMatcherPolicyProvider>()
                .GetPolicies();

            foreach (var matcher in matchers)
            {
                if (matcher is IEndpointSelectorPolicy policy)
                {
                    policy.ApplyAsync(httpContext, context, candidates);
                }
            }

            return Task.CompletedTask;
        }
    }
}