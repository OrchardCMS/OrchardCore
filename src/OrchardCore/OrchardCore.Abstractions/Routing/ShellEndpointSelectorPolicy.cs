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
    /// <summary>
    /// Makes the bridge between the global routing system and a tenant endpoint selector policy.
    /// So that the policy is exposed to the host but it is still instantiated in a tenant scope.
    /// </summary>
    public class ShellEndpointSelectorPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Type _type;
        private int? _order;

        public ShellEndpointSelectorPolicy(IHttpContextAccessor httpContextAccessor, Type type)
        {
            _httpContextAccessor = httpContextAccessor;
            _type = type;
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

        private MatcherPolicy Policy => _httpContextAccessor.HttpContext?.RequestServices
            .GetServices<MatcherPolicy>().Where(m => m.GetType() == _type).FirstOrDefault();

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            return (Policy as IEndpointSelectorPolicy)?.AppliesToEndpoints(endpoints) ?? false;
        }

        public Task ApplyAsync(HttpContext httpContext, EndpointSelectorContext context, CandidateSet candidates)
        {
            return (Policy as IEndpointSelectorPolicy)?.ApplyAsync(httpContext, context, candidates) ?? Task.CompletedTask;
        }
    }
}