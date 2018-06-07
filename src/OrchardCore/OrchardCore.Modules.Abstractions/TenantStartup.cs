using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules
{
    public interface ITenantStartup { }

    public class TenantStartup : StartupBase, ITenantStartup
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TenantStartupActions _actions;

        public TenantStartup(IServiceProvider serviceProvider, TenantStartupActions actions, int order)
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
                configureServices(new TenantServicesBuilder(services, _serviceProvider));
            }
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            foreach (var configure in _actions.ConfigureActions)
            {
                configure(new TenantApplicationBuilder(app), routes);
            }
        }
    }
}
