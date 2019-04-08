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
                    var order = Policy?.Order ?? int.MaxValue;

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
            return Policy?.AppliesToEndpoints(endpoints) ?? false;
        }

        public Task ApplyAsync(HttpContext httpContext, EndpointSelectorContext context, CandidateSet candidates)
        {
            return Policy?.ApplyAsync(httpContext, context, candidates) ?? Task.CompletedTask;
        }
    }
}