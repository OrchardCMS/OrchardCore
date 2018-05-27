using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules
{
    public class OrchardCoreStartupBuilder
    {
        public OrchardCoreStartupBuilder(OrchardCoreBuilder builder)
        {
            Builder = builder;
        }

        public OrchardCoreBuilder Builder { get; }

        internal IDictionary<int, OrchardCoreStartupActions> Actions { get; } =
            new Dictionary<int, OrchardCoreStartupActions>();

        internal OrchardCoreStartupBuilder AddConfigureServices(Action<IServiceCollection, IServiceProvider> configureServicesAction, int order)
        {
            if (!Actions.TryGetValue(order, out var actions))
            {
                actions = Actions[order] = new OrchardCoreStartupActions(order);
            }

            actions.ConfigureServicesActions.Add(configureServicesAction);

            return this;
        }

        internal OrchardCoreStartupBuilder AddConfigure(Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configureAction, int order)
        {
            if (!Actions.TryGetValue(order, out var actions))
            {
                actions = Actions[order] = new OrchardCoreStartupActions(order);
            }

            actions.ConfigureActions.Add(configureAction);

            return this;
        }
    }

    public class OrchardCoreStartupActions
    {
        public OrchardCoreStartupActions(int order)
        {
            Order = order;
        }

        public int Order { get; }

        public IList<Action<IServiceCollection, IServiceProvider>> ConfigureServicesActions { get; set; } =
            new List<Action<IServiceCollection, IServiceProvider>>();

        public IList<Action<IApplicationBuilder, IRouteBuilder, IServiceProvider>> ConfigureActions { get; set; } =
            new List<Action<IApplicationBuilder, IRouteBuilder, IServiceProvider>>();
    }

    public static class OrchardCoreStartupBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services before all modules (unless you specify a higher order).
        /// </summary>
        /// <param name="services"></param>
        public static OrchardCoreStartupBuilder ConfigureServices(this OrchardCoreStartupBuilder startup,
            Action<IServiceCollection, IServiceProvider> configureServices, int order = int.MinValue)
        {
            return startup.AddConfigureServices(configureServices, order);
        }

        /// <summary>
        /// Configure the tenant pipeline before all modules (unless you specify a higher order).
        /// </summary>
        /// <param name="services"></param>
        public static OrchardCoreStartupBuilder Configure(this OrchardCoreStartupBuilder startup,
            Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configure, int order = int.MinValue)
        {
            return startup.AddConfigure(configure, order);
        }
    }
}
