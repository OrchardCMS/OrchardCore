using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Routing
{
    public class ShellNodeBuilderPolicy<T> : MatcherPolicy, IEndpointComparerPolicy, INodeBuilderPolicy where T : MatcherPolicy, IEndpointComparerPolicy, INodeBuilderPolicy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int? _order;

        public ShellNodeBuilderPolicy(IHttpContextAccessor httpContextAccessor)
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

        public IComparer<Endpoint> Comparer => throw new System.NotImplementedException();

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            var policy = Policy;

            if (policy == null)
            {
                return true;
            }

            return policy.AppliesToEndpoints(endpoints);
        }

        public PolicyJumpTable BuildJumpTable(int exitDestination, IReadOnlyList<PolicyJumpTableEdge> edges)
        {
            var policy = Policy;

            if (policy == null)
            {
                //return true;
            }

            return policy.BuildJumpTable(exitDestination, edges);
        }

        public IReadOnlyList<PolicyNodeEdge> GetEdges(IReadOnlyList<Endpoint> endpoints)
        {
            var policy = Policy;

            if (policy == null)
            {
                //return true;
            }

            return policy.GetEdges(endpoints);
        }
    }
}