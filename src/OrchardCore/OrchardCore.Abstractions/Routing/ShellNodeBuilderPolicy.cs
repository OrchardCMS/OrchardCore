using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Routing
{
    public class ShellNodeBuilderPolicy : MatcherPolicy, IEndpointComparerPolicy, INodeBuilderPolicy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _typeFullName;
        private int? _order;

        public ShellNodeBuilderPolicy(IHttpContextAccessor httpContextAccessor, string typeFullName)
        {
            _httpContextAccessor = httpContextAccessor;
            _typeFullName = typeFullName;
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

        private MatcherPolicy Policy =>
            _httpContextAccessor.HttpContext?.RequestServices.GetServices<MatcherPolicy>()
            .Where(m => m.GetType().FullName == _typeFullName).FirstOrDefault();

        public IComparer<Endpoint> Comparer => (Policy as IEndpointComparerPolicy).Comparer;

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            var policy = Policy;

            if (policy == null)
            {
                //return true;
            }

            return (policy as INodeBuilderPolicy).AppliesToEndpoints(endpoints);
        }

        public PolicyJumpTable BuildJumpTable(int exitDestination, IReadOnlyList<PolicyJumpTableEdge> edges)
        {
            var policy = Policy;

            if (policy == null)
            {
                //return true;
            }

            return (policy as INodeBuilderPolicy).BuildJumpTable(exitDestination, edges);
        }

        public IReadOnlyList<PolicyNodeEdge> GetEdges(IReadOnlyList<Endpoint> endpoints)
        {
            var policy = Policy;

            if (policy == null)
            {
                //return true;
            }

            return (policy as INodeBuilderPolicy).GetEdges(endpoints);
        }
    }
}