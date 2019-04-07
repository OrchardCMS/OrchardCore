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

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            var policy = Policy;

            if (policy == null)
            {
                return true;
            }

            return (policy as IEndpointSelectorPolicy).AppliesToEndpoints(endpoints);
        }

        public Task ApplyAsync(HttpContext httpContext, EndpointSelectorContext context, CandidateSet candidates)
        {
            var policy = Policy;

            if (policy == null)
            {
                return Task.CompletedTask;
            }

            return (policy as IEndpointSelectorPolicy).ApplyAsync(httpContext, context, candidates);
        }
    }
}