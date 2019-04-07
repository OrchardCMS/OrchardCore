using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Routing
{
    public class ShellEndpointSelectorPolicy<T> : MatcherPolicy, IEndpointSelectorPolicy where T : MatcherPolicy, IEndpointSelectorPolicy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int? _order;

        public ShellEndpointSelectorPolicy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override int Order
        {
            get
            {
                if (!_order.HasValue)
                {
                    var order = Policy?.Order ?? 0;

                    lock (this)
                    {
                        _order = order;
                    }
                }

                return _order.Value;
            }
        }

        private T Policy => _httpContextAccessor.HttpContext?.RequestServices
            .GetServices<MatcherPolicy>().OfType<T>().FirstOrDefault();

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            var policy = Policy;

            if (policy == null)
            {
                return true;
            }

            return policy.AppliesToEndpoints(endpoints);
        }

        public Task ApplyAsync(HttpContext httpContext, EndpointSelectorContext context, CandidateSet candidates)
        {
            var policy = Policy;

            if (policy == null)
            {
                return Task.CompletedTask;
            }

            return policy.ApplyAsync(httpContext, context, candidates);
        }
    }
}