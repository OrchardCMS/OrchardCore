using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public class TenantStartupActions
    {
        public TenantStartupActions(int order)
        {
            Order = order;
        }

        public int Order { get; }

        public ICollection<Action<TenantServicesBuilder>> ConfigureServicesActions { get; set; } =
            new List<Action<TenantServicesBuilder>>();

        public ICollection<Action<TenantApplicationBuilder, IRouteBuilder>> ConfigureActions { get; set; } =
            new List<Action<TenantApplicationBuilder, IRouteBuilder>>();
    }
}
