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
                    var order = Policy?.Order ?? int.MaxValue;

                    lock (this)
                    {
                        _order = order;
                    }
                }

                return _order.Value;
            }
        }

        private MatcherPolicy Policy => _httpContextAccessor.HttpContext?.RequestServices.GetServices<MatcherPolicy>()
            .Where(m => m.GetType().FullName == _typeFullName).FirstOrDefault();

        public IComparer<Endpoint> Comparer => (Policy as IEndpointComparerPolicy)
            ?.Comparer ?? new ZeroPolicyComparer();

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            return (Policy as INodeBuilderPolicy)?.AppliesToEndpoints(endpoints) ?? false;
        }

        public IReadOnlyList<PolicyNodeEdge> GetEdges(IReadOnlyList<Endpoint> endpoints)
        {
            return (Policy as INodeBuilderPolicy)?.GetEdges(endpoints) ?? new List<PolicyNodeEdge>();
        }

        public PolicyJumpTable BuildJumpTable(int exitDestination, IReadOnlyList<PolicyJumpTableEdge> edges)
        {
            return (Policy as INodeBuilderPolicy)?.BuildJumpTable(exitDestination, edges) ?? new ZeroPolicyJumpTable(exitDestination);
        }
    }

    internal class ZeroPolicyJumpTable : PolicyJumpTable
    {
        private readonly int _exitDestination;

        public ZeroPolicyJumpTable(int exitDestination) => _exitDestination = exitDestination;

        public override int GetDestination(HttpContext httpContext) => _exitDestination;
    }

    internal class ZeroPolicyComparer : IComparer<Endpoint>
    {
        public int Compare(Endpoint x, Endpoint y) => 0;
    }
}