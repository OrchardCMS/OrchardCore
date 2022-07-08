using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public class OrchardCoreBuilder
    {
        private Dictionary<int, StartupActions> _actions { get; } = new Dictionary<int, StartupActions>();

        public OrchardCoreBuilder(IServiceCollection services)
        {
            ApplicationServices = services;
        }

        public IServiceCollection ApplicationServices { get; }

        public OrchardCoreBuilder RegisterStartup<T>() where T : class, IStartup
        {
            ApplicationServices.AddTransient<IStartup, T>();
            return this;
        }

        /// <summary>
        /// This method gets called for each tenant. Use this method to add services to the container.
        /// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="configure">The action to execute when configuring the services for a tenant.</param>
        /// <param name="order">The order of the action to execute. Lower values will be executed first.</param>
        public OrchardCoreBuilder ConfigureServices(Action<IServiceCollection, IServiceProvider> configure, int order = 0)
        {
            if (!_actions.TryGetValue(order, out var actions))
            {
                actions = _actions[order] = new StartupActions(order);

                ApplicationServices.AddTransient<IStartup>(sp => new StartupActionsStartup(
                    sp.GetRequiredService<IServiceProvider>(), actions, order));
            }

            actions.ConfigureServicesActions.Add(configure);

            return this;
        }

        /// <summary>
        /// This method gets called for each tenant. Use this method to add services to the container.
        /// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="configure">The action to execute when configuring the services for a tenant.</param>
        /// <param name="order">The order of the action to execute. Lower values will be executed first.</param>
        public OrchardCoreBuilder ConfigureServices(Action<IServiceCollection> configure, int order = 0)
        {
            return ConfigureServices((s, sp) => configure(s), order);
        }

        /// <summary>
        /// This method gets called for each tenant. Use this method to configure the request's pipeline.
        /// </summary>
        /// <param name="configure">The action to execute when configuring the request's pipeline for a tenant.</param>
        /// <param name="order">The order of the action to execute. Lower values will be executed first.</param>
        public OrchardCoreBuilder Configure(Action<IApplicationBuilder, IEndpointRouteBuilder, IServiceProvider> configure, int order = 0)
        {
            if (!_actions.TryGetValue(order, out var actions))
            {
                actions = _actions[order] = new StartupActions(order);

                ApplicationServices.AddTransient<IStartup>(sp => new StartupActionsStartup(
                    sp.GetRequiredService<IServiceProvider>(), actions, order));
            }

            actions.ConfigureActions.Add(configure);

            return this;
        }

        /// <summary>
        /// This method gets called for each tenant. Use this method to configure the request's pipeline.
        /// </summary>
        /// <param name="configure">The action to execute when configuring the request's pipeline for a tenant.</param>
        /// <param name="order">The order of the action to execute. Lower values will be executed first.</param>
        public OrchardCoreBuilder Configure(Action<IApplicationBuilder, IEndpointRouteBuilder> configure, int order = 0)
        {
            return Configure((app, routes, sp) => configure(app, routes), order);
        }

        /// <summary>
        /// This method gets called for each tenant. Use this method to configure the request's pipeline.
        /// </summary>
        /// <param name="configure">The action to execute when configuring the request's pipeline for a tenant.</param>
        /// <param name="order">The order of the action to execute. Lower values will be executed first.</param>
        public OrchardCoreBuilder Configure(Action<IApplicationBuilder> configure, int order = 0)
        {
            return Configure((app, routes, sp) => configure(app), order);
        }

        public OrchardCoreBuilder EnableFeature(string id)
        {
            return ConfigureServices(services =>
            {
                for (var index = 0; index < services.Count; index++)
                {
                    var service = services[index];
                    if (service.ImplementationInstance is ShellFeature feature &&
                        string.Equals(feature.Id, id, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }

                services.AddSingleton(new ShellFeature(id, alwaysEnabled: true));
            });
        }
    }
}
