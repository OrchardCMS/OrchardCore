using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Routing
{
    internal class ShellMatcherPolicyProvider
    {
        private readonly IEnumerable<MatcherPolicy> _hostMatcherPolicies;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShellMatcherPolicyProvider(
            IEnumerable<MatcherPolicy> hostMatcherPolicies,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostMatcherPolicies = hostMatcherPolicies;
        }

        public IEnumerable<IEndpointSelectorPolicy> GetEndpointSelectorPolicies()
        {
            // Retrieve all tenant level matcher policies.
            return (_httpContextAccessor.HttpContext?.RequestServices
                .GetServices<MatcherPolicy>()
                .Except(_hostMatcherPolicies)
                .Where(matcher => matcher is IEndpointSelectorPolicy)
                .OrderBy(matcher => matcher.Order)
                .Select(matcher => matcher as IEndpointSelectorPolicy)
                ?? Enumerable.Empty<IEndpointSelectorPolicy>());
        }
    }
}