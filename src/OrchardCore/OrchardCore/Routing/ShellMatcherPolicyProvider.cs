using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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

        public IEnumerable<MatcherPolicy> GetPolicies()
        {
            // Retrieve all tenant level matcher policies.
            return (_httpContextAccessor.HttpContext?.RequestServices
                .GetServices<MatcherPolicy>()
                .OrderBy(m => m.Order)
                ?? Enumerable.Empty<MatcherPolicy>())
                .Except(_hostMatcherPolicies);
        }
    }
}