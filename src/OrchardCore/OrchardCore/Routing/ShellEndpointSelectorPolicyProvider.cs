using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Routing
{
    internal class ShellEndpointSelectorPolicyProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<MatcherPolicy> _hostMatchers;

        public ShellEndpointSelectorPolicyProvider(IHttpContextAccessor httpContextAccessor, IServiceProvider services)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostMatchers = services.GetServices<MatcherPolicy>().ToList();
        }

        public IEnumerable<MatcherPolicy> GetPolicies()
        {
            return (_httpContextAccessor.HttpContext?.RequestServices
                .GetServices<MatcherPolicy>()
                .OrderBy(m => m.Order)
                ?? Enumerable.Empty<MatcherPolicy>())
                .Except(_hostMatchers);
        }
    }
}