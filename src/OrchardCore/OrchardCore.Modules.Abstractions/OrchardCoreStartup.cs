using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules
{
    public interface IOrchardCoreStartup { }

    public class OrchardCoreStartup : StartupBase, IOrchardCoreStartup
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly OrchardCoreStartupActions _actions;

        public OrchardCoreStartup(IServiceProvider serviceProvider, OrchardCoreStartupActions actions, int order)
        {
            _serviceProvider = serviceProvider;
            _actions = actions;
            Order = order;
        }

        public override int Order { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            foreach (var configureServices in _actions.ConfigureServicesActions)
            {
                configureServices(services, _serviceProvider);
            }
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            foreach (var configure in _actions.ConfigureActions)
            {
                configure(app, routes, serviceProvider);
            }
        }
    }
}
