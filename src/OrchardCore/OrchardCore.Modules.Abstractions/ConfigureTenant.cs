using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules
{
    public interface IConfigureTenant { }

    public class ConfigureTenant : StartupBase, IConfigureTenant
    {
        public ConfigureTenant(Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configure, int order)
        {
            configureAction = configure;
            Order = order;
        }

        public override int Order { get; }

        public Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configureAction { get; }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            configureAction?.Invoke(app, routes, serviceProvider);
        }
    }

    public class ConfigureTenantServices : StartupBase, IConfigureTenant
    {
        public ConfigureTenantServices(IServiceProvider serviceProvider, Action<IServiceCollection, IServiceProvider> configureServices, int order)
        {
            ServiceProvider = serviceProvider;
            configureServicesAction = configureServices;
            Order = order;
        }

        public override int Order { get; }

        public Action<IServiceCollection, IServiceProvider> configureServicesAction { get; }

        public IServiceProvider ServiceProvider { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            configureServicesAction?.Invoke(services, ServiceProvider);
        }
    }

    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Configure the tenant pipeline before all modules (unless you specify a higher order).
        /// </summary>
        /// <param name="services"></param>
        public static OrchardCoreBuilder ConfigureTenant(this OrchardCoreBuilder builder,
            Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configure, int order = int.MinValue)
        {
            builder.Services.AddTransient<IStartup>(sp => new ConfigureTenant(configure, order));
            return builder;
        }

        /// <summary>
        /// Configure the tenant pipeline after all modules.
        /// </summary>
        /// <param name="services"></param>
        public static OrchardCoreBuilder PostConfigureTenant(this OrchardCoreBuilder builder,
            Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configure)
        {
            return builder.ConfigureTenant(configure, int.MaxValue);
        }

        /// <summary>
        /// Adds tenant level services before all modules (unless you specify a higher order).
        /// </summary>
        /// <param name="services"></param>
        public static OrchardCoreBuilder ConfigureTenantServices(this OrchardCoreBuilder builder,
            Action<IServiceCollection, IServiceProvider> configureServices, int order = int.MinValue)
        {
            builder.Services.AddTransient<IStartup>(sp => new ConfigureTenantServices(
                sp.GetRequiredService<IServiceProvider>(),
                configureServices,
                order));

            return builder;
        }

        /// <summary>
        /// Adds tenant level services after all modules.
        /// </summary>
        /// <param name="services"></param>
        public static OrchardCoreBuilder PostConfigureTenantServices(this OrchardCoreBuilder builder,
            Action<IServiceCollection, IServiceProvider> configureServices)
        {
            return builder.ConfigureTenantServices(configureServices, int.MaxValue);
        }
    }
}
