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

        public IList<Action<TenantServicesBuilder, IServiceProvider>> ConfigureServicesActions { get; set; } =
            new List<Action<TenantServicesBuilder, IServiceProvider>>();

        public IList<Action<TenantApplicationBuilder, IRouteBuilder, IServiceProvider>> ConfigureActions { get; set; } =
            new List<Action<TenantApplicationBuilder, IRouteBuilder, IServiceProvider>>();
    }
}
