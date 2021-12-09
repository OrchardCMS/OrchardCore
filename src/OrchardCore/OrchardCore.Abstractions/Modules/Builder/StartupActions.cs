using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class StartupActions
    {
        public StartupActions(int order)
        {
            Order = order;
        }

        public int Order { get; }

        public ICollection<Action<IServiceCollection, IServiceProvider>> ConfigureServicesActions { get; } = new List<Action<IServiceCollection, IServiceProvider>>();

        public ICollection<Action<IApplicationBuilder, IEndpointRouteBuilder, IServiceProvider>> ConfigureActions { get; } = new List<Action<IApplicationBuilder, IEndpointRouteBuilder, IServiceProvider>>();
    }
}
