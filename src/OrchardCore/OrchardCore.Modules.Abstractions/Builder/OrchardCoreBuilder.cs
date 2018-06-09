using System;
using System.Collections.Generic;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public class OrchardCoreBuilder
    {
        private Dictionary<int, TenantStartupActions> _actions { get; } = new Dictionary<int, TenantStartupActions>();

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
        public OrchardCoreBuilder ConfigureServices(ConfigureServicesDelegate configure, int order = 0)
        {
            if (!_actions.TryGetValue(order, out var actions))
            {
                actions = _actions[order] = new TenantStartupActions(order);

                ApplicationServices.AddTransient<IStartup>(sp => new TenantStartup(
                    sp.GetRequiredService<IServiceProvider>(), actions, order));
            }

            actions.ConfigureServicesActions.Add(configure);

            return this;
        }

        public OrchardCoreBuilder Configure(ConfigureDelegate configure, int order = 0)
        {
            if (!_actions.TryGetValue(order, out var actions))
            {
                actions = _actions[order] = new TenantStartupActions(order);

                ApplicationServices.AddTransient<IStartup>(sp => new TenantStartup(
                    sp.GetRequiredService<IServiceProvider>(), actions, order));
            }

            actions.ConfigureActions.Add(configure);

            return this;
        }
    }
}
