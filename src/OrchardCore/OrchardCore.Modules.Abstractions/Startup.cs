using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules
{
    public abstract class StartupBase : IStartup
    {
        /// <inheritdoc />
        public virtual int Order => 0;

        /// <inheritdoc />
        public virtual void ConfigureServices(IServiceCollection services)
        {
        }

        /// <inheritdoc />
        public virtual void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }

    public class ConfigureTenantServices : StartupBase
    {
        public ConfigureTenantServices(Action<IServiceCollection> configureServices)
        {
            configureServicesAction = configureServices;
        }

        public override int Order => 10000;
        public Action<IServiceCollection> configureServicesAction { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            configureServicesAction?.Invoke(services);
        }
    }

    public class ConfigureTenant : StartupBase
    {
        public ConfigureTenant(Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configure)
        {
            configureAction = configure;
        }

        public override int Order => -10000;
        public Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configureAction { get; }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            configureAction?.Invoke(app, routes, serviceProvider);
        }
    }

    public class ConfigureTenantServices<TDep> : StartupBase where TDep : class
    {
        public ConfigureTenantServices(TDep dependency, Action<IServiceCollection, TDep> configureServices)
        {
            configureServicesAction = configureServices;
            Dependency = dependency;
        }

        public override int Order => 10000;
        public Action<IServiceCollection, TDep> configureServicesAction { get; }

        public TDep Dependency { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            configureServicesAction?.Invoke(services, Dependency);
        }
    }

    public class ConfigureTenantServices<TDep1, TDep2> : StartupBase where TDep1 : class where TDep2: class
    {
        public ConfigureTenantServices(TDep1 dependency1, TDep2 dependency2, Action<IServiceCollection, TDep1, TDep2> configureServices)
        {
            configureServicesAction = configureServices;
            Dependency1 = dependency1;
            Dependency2 = dependency2;
        }

        public override int Order => 10000;
        public Action<IServiceCollection, TDep1, TDep2> configureServicesAction { get; }

        public TDep1 Dependency1 { get; }
        public TDep2 Dependency2 { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            configureServicesAction?.Invoke(services, Dependency1, Dependency2);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureTenantServices(this IServiceCollection services,
            Action<IServiceCollection> configureServices)
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices(configureServices));
            return services;
        }

        public static IServiceCollection ConfigureTenant(this IServiceCollection services,
            Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configure)
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenant(configure));
            return services;
        }

        public static IServiceCollection ConfigureTenantServices<TDep>(this IServiceCollection services,
            Action<IServiceCollection, TDep> configureServices) where TDep : class
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices<TDep>(
                sp.GetRequiredService<TDep>(), configureServices));
            return services;
        }

        public static IServiceCollection ConfigureTenantServices<TDep1, TDep2>(this IServiceCollection services,
            Action<IServiceCollection, TDep1, TDep2> configureServices) where TDep1 : class where TDep2 : class
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices<TDep1, TDep2>(
                sp.GetRequiredService<TDep1>(), sp.GetRequiredService<TDep2>(), configureServices));
            return services;
        }
    }
}
