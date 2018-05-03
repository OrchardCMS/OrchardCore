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

    public class TenantStartup : StartupBase
    {
        public TenantStartup(Action<IServiceCollection> configureServices,
            Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configure)
        {
            configureServicesAction = configureServices;
            configureAction = configure;
        }

        public Action<IServiceCollection> configureServicesAction { get; }
        public Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configureAction { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            configureServicesAction?.Invoke(services);
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            configureAction?.Invoke(app, routes, serviceProvider);
        }
    }

    public class TenantStartup<TDep> : StartupBase where TDep : class
    {
        public TenantStartup(TDep dependency, Action<IServiceCollection, TDep> configureServices,
            Action<IApplicationBuilder, IRouteBuilder, IServiceProvider, TDep> configure)
        {
            configureServicesAction = configureServices;
            configureAction = configure;
            Dependency = dependency;
        }

        public Action<IServiceCollection, TDep> configureServicesAction { get; }
        public Action<IApplicationBuilder, IRouteBuilder, IServiceProvider, TDep> configureAction { get; }

        public TDep Dependency { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            configureServicesAction?.Invoke(services, Dependency);
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            configureAction?.Invoke(app, routes, serviceProvider, Dependency);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureTenantServices(this IServiceCollection services,
            Action<IServiceCollection> configureServices,
            Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configure)
        {
            services.AddTransient<IStartup>(sp => new TenantStartup(configureServices, configure));
            return services;
        }

        public static IServiceCollection ConfigureTenant(this IServiceCollection services,
            Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configure)
        {
            services.AddTransient<IStartup>(sp => new TenantStartup(null, configure));
            return services;
        }

        public static IServiceCollection ConfigureTenantServices<TDep>(this IServiceCollection services,
            Action<IServiceCollection, TDep> configureServices) where TDep : class
        {
            services.AddTransient<IStartup>(sp => new TenantStartup<TDep>(sp.GetRequiredService<TDep>(), configureServices, null));
            return services;
        }
    }
}
