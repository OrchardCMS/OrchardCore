using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Routing
{
    public class ShellEndpointSelectorPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _typeFullName;
        private int? _order;

        public ShellEndpointSelectorPolicy(IHttpContextAccessor httpContextAccessor, string typeFullName)
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