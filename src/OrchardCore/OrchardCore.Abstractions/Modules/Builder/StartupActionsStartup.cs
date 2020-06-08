using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules
{
    /// <summary>
    /// Represents a fake Startup class that is composed of Configure and ConfigureServices lambdas.
    /// </summary>
    internal class StartupActionsStartup : StartupBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly StartupActions _actions;

        public StartupActionsStartup(IServiceProvider serviceProvider, StartupActions actions, int order)
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
                configureServices?.Invoke(services, _serviceProvider);
            }
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            foreach (var configure in _actions.ConfigureActions)
            {
                configure?.Invoke(app, routes, serviceProvider);
            }
        }
    }
}
