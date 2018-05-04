using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules
{
    public class ConfigureTenant : StartupBase
    {
        public ConfigureTenant(Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configure)
        {
            configureAction = configure;
        }

        public override int Order { get; internal set; } = -10000;

        public Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configureAction { get; }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            configureAction?.Invoke(app, routes, serviceProvider);
        }
    }

    public class ConfigureTenantServices : StartupBase
    {
        public ConfigureTenantServices(Action<IServiceCollection> configureServices)
        {
            configureServicesAction = configureServices;
        }

        public override int Order { get; internal set; } = -10000;

        public Action<IServiceCollection> configureServicesAction { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            configureServicesAction?.Invoke(services);
        }
    }

    public class ConfigureTenantServices<TDep> : StartupBase
        where TDep : class
    {
        public ConfigureTenantServices(TDep dependency, Action<IServiceCollection, TDep> configureServices)
        {
            configureServicesAction = configureServices;
            Dependency = dependency;
        }

        public override int Order { get; internal set; } = -10000;

        public Action<IServiceCollection, TDep> configureServicesAction { get; }

        public TDep Dependency { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            configureServicesAction?.Invoke(services, Dependency);
        }
    }

    public class ConfigureTenantServices<TDep1, TDep2> : StartupBase
        where TDep1 : class
        where TDep2: class
    {
        public ConfigureTenantServices(TDep1 dependency1, TDep2 dependency2,
            Action<IServiceCollection, TDep1, TDep2> configureServices)
        {
            configureServicesAction = configureServices;
            Dependency1 = dependency1;
            Dependency2 = dependency2;
        }

        public override int Order { get; internal set; } = -10000;

        public Action<IServiceCollection, TDep1, TDep2> configureServicesAction { get; }

        public TDep1 Dependency1 { get; }
        public TDep2 Dependency2 { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            configureServicesAction?.Invoke(services, Dependency1, Dependency2);
        }
    }

    public class ConfigureTenantServices<TDep1, TDep2, TDep3> : StartupBase
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
    {
        public ConfigureTenantServices(TDep1 dependency1, TDep2 dependency2, TDep3 dependency3,
            Action<IServiceCollection, TDep1, TDep2, TDep3> configureServices)
        {
            configureServicesAction = configureServices;
            Dependency1 = dependency1;
            Dependency2 = dependency2;
            Dependency3 = dependency3;
        }

        public override int Order { get; internal set; } = -10000;

        public Action<IServiceCollection, TDep1, TDep2, TDep3> configureServicesAction { get; }

        public TDep1 Dependency1 { get; }
        public TDep2 Dependency2 { get; }
        public TDep3 Dependency3 { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            configureServicesAction?.Invoke(services, Dependency1, Dependency2, Dependency3);
        }
    }

    public class ConfigureTenantServices<TDep1, TDep2, TDep3, TDep4> : StartupBase
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TDep4 : class
    {
        public ConfigureTenantServices(TDep1 dependency1, TDep2 dependency2, TDep3 dependency3, TDep4 dependency4,
            Action<IServiceCollection, TDep1, TDep2, TDep3, TDep4> configureServices)
        {
            configureServicesAction = configureServices;
            Dependency1 = dependency1;
            Dependency2 = dependency2;
            Dependency3 = dependency3;
            Dependency4 = dependency4;
        }

        public override int Order { get; internal set; } = -10000;

        public Action<IServiceCollection, TDep1, TDep2, TDep3, TDep4> configureServicesAction { get; }

        public TDep1 Dependency1 { get; }
        public TDep2 Dependency2 { get; }
        public TDep3 Dependency3 { get; }
        public TDep4 Dependency4 { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            configureServicesAction?.Invoke(services, Dependency1, Dependency2, Dependency3, Dependency4);
        }
    }

    public class ConfigureTenantServices<TDep1, TDep2, TDep3, TDep4, TDep5> : StartupBase
        where TDep1 : class
        where TDep2 : class
        where TDep3 : class
        where TDep4 : class
        where TDep5 : class
    {
        public ConfigureTenantServices(TDep1 dependency1, TDep2 dependency2, TDep3 dependency3, TDep4 dependency4, TDep5 dependency5,
            Action<IServiceCollection, TDep1, TDep2, TDep3, TDep4, TDep5> configureServices)
        {
            configureServicesAction = configureServices;
            Dependency1 = dependency1;
            Dependency2 = dependency2;
            Dependency3 = dependency3;
            Dependency4 = dependency4;
            Dependency5 = dependency5;
        }

        public override int Order { get; internal set; } = -10000;

        public Action<IServiceCollection, TDep1, TDep2, TDep3, TDep4, TDep5> configureServicesAction { get; }

        public TDep1 Dependency1 { get; }
        public TDep2 Dependency2 { get; }
        public TDep3 Dependency3 { get; }
        public TDep4 Dependency4 { get; }
        public TDep5 Dependency5 { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            configureServicesAction?.Invoke(services, Dependency1, Dependency2, Dependency3, Dependency4, Dependency5);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureTenant(this IServiceCollection services,
            Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configure)
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenant(configure));
            return services;
        }

        public static IServiceCollection PostConfigureTenant(this IServiceCollection services,
            Action<IApplicationBuilder, IRouteBuilder, IServiceProvider> configure)
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenant(configure) { Order = 10000 });
            return services;
        }

        public static IServiceCollection ConfigureTenantServices(this IServiceCollection services,
            Action<IServiceCollection> configureServices)
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices(configureServices));
            return services;
        }

        public static IServiceCollection PostConfigureTenantServices(this IServiceCollection services,
            Action<IServiceCollection> configureServices)
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices(configureServices) { Order = 10000 });
            return services;
        }

        public static IServiceCollection ConfigureTenantServices<TDep>(this IServiceCollection services,
            Action<IServiceCollection, TDep> configureServices)
            where TDep : class
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices<TDep>(
                sp.GetRequiredService<TDep>(),
                configureServices));

            return services;
        }

        public static IServiceCollection PostConfigureTenantServices<TDep>(this IServiceCollection services,
            Action<IServiceCollection, TDep> configureServices)
            where TDep : class
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices<TDep>(
                sp.GetRequiredService<TDep>(),
                configureServices)
                { Order = 10000 });

            return services;
        }

        public static IServiceCollection ConfigureTenantServices<TDep1, TDep2>(this IServiceCollection services,
            Action<IServiceCollection, TDep1, TDep2> configureServices)
            where TDep1 : class
            where TDep2 : class
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices<TDep1, TDep2>(
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                configureServices));

            return services;
        }

        public static IServiceCollection PostConfigureTenantServices<TDep1, TDep2>(this IServiceCollection services,
            Action<IServiceCollection, TDep1, TDep2> configureServices)
            where TDep1 : class
            where TDep2 : class
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices<TDep1, TDep2>(
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                configureServices)
                { Order = 10000 });

            return services;
        }

        public static IServiceCollection ConfigureTenantServices<TDep1, TDep2, TDep3>(this IServiceCollection services,
            Action<IServiceCollection, TDep1, TDep2, TDep3> configureServices)
            where TDep1 : class
            where TDep2 : class
            where TDep3 : class
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices<TDep1, TDep2, TDep3>(
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                configureServices));

            return services;
        }

        public static IServiceCollection PostConfigureTenantServices<TDep1, TDep2, TDep3>(this IServiceCollection services,
            Action<IServiceCollection, TDep1, TDep2, TDep3> configureServices)
            where TDep1 : class
            where TDep2 : class
            where TDep3 : class
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices<TDep1, TDep2, TDep3>(
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                configureServices)
                { Order = 10000 });

            return services;
        }

        public static IServiceCollection ConfigureTenantServices<TDep1, TDep2, TDep3, TDep4>(this IServiceCollection services,
            Action<IServiceCollection, TDep1, TDep2, TDep3, TDep4> configureServices)
            where TDep1 : class
            where TDep2 : class
            where TDep3 : class
            where TDep4 : class
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices<TDep1, TDep2, TDep3, TDep4>(
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<TDep4>(),
                configureServices));

            return services;
        }

        public static IServiceCollection PostConfigureTenantServices<TDep1, TDep2, TDep3, TDep4>(this IServiceCollection services,
            Action<IServiceCollection, TDep1, TDep2, TDep3, TDep4> configureServices)
            where TDep1 : class
            where TDep2 : class
            where TDep3 : class
            where TDep4 : class
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices<TDep1, TDep2, TDep3, TDep4>(
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<TDep4>(),
                configureServices)
                { Order = 10000 });

            return services;
        }

        public static IServiceCollection ConfigureTenantServices<TDep1, TDep2, TDep3, TDep4, TDep5>(this IServiceCollection services,
            Action<IServiceCollection, TDep1, TDep2, TDep3, TDep4, TDep5> configureServices)
            where TDep1 : class
            where TDep2 : class
            where TDep3 : class
            where TDep4 : class
            where TDep5 : class
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices<TDep1, TDep2, TDep3, TDep4, TDep5>(
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<TDep4>(),
                sp.GetRequiredService<TDep5>(),
                configureServices));

            return services;
        }

        public static IServiceCollection PostConfigureTenantServices<TDep1, TDep2, TDep3, TDep4, TDep5>(this IServiceCollection services,
            Action<IServiceCollection, TDep1, TDep2, TDep3, TDep4, TDep5> configureServices)
            where TDep1 : class
            where TDep2 : class
            where TDep3 : class
            where TDep4 : class
            where TDep5 : class
        {
            services.AddTransient<IStartup>(sp => new ConfigureTenantServices<TDep1, TDep2, TDep3, TDep4, TDep5>(
                sp.GetRequiredService<TDep1>(),
                sp.GetRequiredService<TDep2>(),
                sp.GetRequiredService<TDep3>(),
                sp.GetRequiredService<TDep4>(),
                sp.GetRequiredService<TDep5>(),
                configureServices)
                { Order = 10000 });

            return services;
        }
    }
}
