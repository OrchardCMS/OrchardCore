using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

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

        internal TenantStartupBuilder AddConfigureServices(Action<TenantServicesBuilder, IServiceProvider> configureServicesAction, int order)
        {
            if (!Actions.TryGetValue(order, out var actions))
            {
                actions = Actions[order] = new TenantStartupActions(order);
            }

            actions.ConfigureServicesActions.Add(configureServicesAction);

            return this;
        }

        internal TenantStartupBuilder AddConfigure(Action<TenantApplicationBuilder, IRouteBuilder, IServiceProvider> configureAction, int order)
        {
            if (!Actions.TryGetValue(order, out var actions))
            {
                actions = Actions[order] = new TenantStartupActions(order);
            }

            actions.ConfigureActions.Add(configureAction);

            return this;
        }
    }
}
