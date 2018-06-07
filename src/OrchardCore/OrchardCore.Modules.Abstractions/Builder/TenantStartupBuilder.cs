using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public class TenantStartupBuilder
    {
        public TenantStartupBuilder(OrchardCoreBuilder builder)
        {
            Builder = builder;
        }

        public OrchardCoreBuilder Builder { get; }

        internal IDictionary<int, TenantStartupActions> Actions { get; } = new Dictionary<int, TenantStartupActions>();

        internal TenantStartupBuilder AddConfigureServices(Action<TenantServicesBuilder> configureServicesAction, int order)
        {
            if (!Actions.TryGetValue(order, out var actions))
            {
                actions = Actions[order] = new TenantStartupActions(order);

                Builder.Services.AddTransient<IStartup>(sp => new TenantStartup(
                    sp.GetRequiredService<IServiceProvider>(), actions, order));
            }

            actions.ConfigureServicesActions.Add(configureServicesAction);

            return this;
        }

        internal TenantStartupBuilder AddConfigure(Action<TenantApplicationBuilder, IRouteBuilder> configureAction, int order)
        {
            if (!Actions.TryGetValue(order, out var actions))
            {
                actions = Actions[order] = new TenantStartupActions(order);

                Builder.Services.AddTransient<IStartup>(sp => new TenantStartup(
                    sp.GetRequiredService<IServiceProvider>(), actions, order));
            }

            actions.ConfigureActions.Add(configureAction);

            return this;
        }
    }
}
