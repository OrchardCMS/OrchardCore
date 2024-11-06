using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Server.DataProtection;
using OpenIddict.Validation;
using OpenIddict.Validation.AspNetCore;
using OpenIddict.Validation.DataProtection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Deployment;
using OrchardCore.OpenId.Drivers;
using OrchardCore.OpenId.Migrations;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Services.Handlers;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.Tasks;
using OrchardCore.Recipes;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.OpenId;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Register the OpenIddict core services and the Orchard migrations, managers and default YesSql stores.
        // The default YesSql stores can be replaced by another database by referencing the corresponding
        // OpenIddict package (e.g OpenIddict.EntityFrameworkCore) and registering it in the options.
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.AddOrchardMigrations()
                       .UseOrchardManagers()
                       .UseYesSql();
            });

        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();
    }
}

[Feature(OpenIdConstants.Features.Client)]
public sealed class ClientStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddSingleton<IOpenIdClientService, OpenIdClientService>();

        // Note: the following services are registered using TryAddEnumerable to prevent duplicate registrations.
        services.TryAddEnumerable(new[]
        {
            ServiceDescriptor.Scoped<IDisplayDriver<ISite>, OpenIdClientSettingsDisplayDriver>(),
        });

        services.AddRecipeExecutionStep<OpenIdClientSettingsStep>();
        // Register the options initializers required by the OpenID Connect client handler.
        services.TryAddEnumerable(new[]
        {
            // Orchard-specific initializers:
            ServiceDescriptor.Singleton<IConfigureOptions<AuthenticationOptions>, OpenIdClientConfiguration>(),
            ServiceDescriptor.Singleton<IConfigureOptions<OpenIdConnectOptions>, OpenIdClientConfiguration>(),

            // Built-in initializers:
            ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>()
        });
    }
}

[Feature(OpenIdConstants.Features.Server)]
public sealed class ServerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOpenIddict()
            .AddServer(options =>
            {
                options.UseAspNetCore();
                options.UseDataProtection();
            });

        services.TryAddSingleton<IOpenIdServerService, OpenIdServerService>();

        services.AddDataMigration<DefaultScopesMigration>();
        // Note: the following services are registered using TryAddEnumerable to prevent duplicate registrations.
        services.TryAddEnumerable(new[]
        {
            ServiceDescriptor.Scoped<IRoleRemovedEventHandler, OpenIdApplicationRoleRemovedEventHandler>(),
            ServiceDescriptor.Scoped<IDisplayDriver<OpenIdServerSettings>, OpenIdServerSettingsDisplayDriver>(),

            ServiceDescriptor.Singleton<IBackgroundTask, OpenIdBackgroundTask>()
        });

        services.AddRecipeExecutionStep<OpenIdServerSettingsStep>()
            .AddRecipeExecutionStep<OpenIdApplicationStep>()
            .AddRecipeExecutionStep<OpenIdScopeStep>();

        // Note: the OpenIddict ASP.NET host adds an authentication options initializer that takes care of
        // registering the server ASP.NET Core handler. Yet, it MUST NOT be registered at this stage
        // as it is lazily registered by OpenIdServerConfiguration only after checking the OpenID server
        // settings are valid and can be safely used in this tenant without causing runtime exceptions.
        // To prevent that, the initializer is manually removed from the services collection of the tenant.
        services.RemoveAll<IConfigureOptions<AuthenticationOptions>, OpenIddictServerAspNetCoreConfiguration>();

        services.TryAddEnumerable(new[]
        {
            ServiceDescriptor.Singleton<IConfigureOptions<AuthenticationOptions>, OpenIdServerConfiguration>(),
            ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictServerOptions>, OpenIdServerConfiguration>(),
            ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictServerAspNetCoreOptions>, OpenIdServerConfiguration>(),
            ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictServerDataProtectionOptions>, OpenIdServerConfiguration>()
        });
    }

    public override async ValueTask ConfigureAsync(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var settings = await GetServerSettingsAsync();
        if (settings == null)
        {
            return;
        }

        if (settings.AuthorizationEndpointPath.HasValue)
        {
            routes.MapAreaControllerRoute(
                name: "Access.Authorize",
                areaName: typeof(Startup).Namespace,
                pattern: settings.AuthorizationEndpointPath.Value,
                defaults: new { controller = "Access", action = "Authorize" }
            );
        }

        if (settings.LogoutEndpointPath.HasValue)
        {
            routes.MapAreaControllerRoute(
                name: "Access.Logout",
                areaName: typeof(Startup).Namespace,
                pattern: settings.LogoutEndpointPath.Value,
                defaults: new { controller = "Access", action = "Logout" }
            );
        }

        if (settings.TokenEndpointPath.HasValue)
        {
            routes.MapAreaControllerRoute(
                name: "Access.Token",
                areaName: typeof(Startup).Namespace,
                pattern: settings.TokenEndpointPath.Value,
                defaults: new { controller = "Access", action = "Token" }
            );
        }

        if (settings.UserinfoEndpointPath.HasValue)
        {
            routes.MapAreaControllerRoute(
                name: "UserInfo.Me",
                areaName: typeof(Startup).Namespace,
                pattern: settings.UserinfoEndpointPath.Value,
                defaults: new { controller = "UserInfo", action = "Me" }
            );
        }

        async Task<OpenIdServerSettings> GetServerSettingsAsync()
        {
            // Note: the OpenID server service is registered as a singleton service and thus can be
            // safely used with the non-scoped/root service provider available at this stage.
            var service = serviceProvider.GetRequiredService<IOpenIdServerService>();

            var configuration = await service.GetSettingsAsync();
            if ((await service.ValidateSettingsAsync(configuration)).Any(result => result != ValidationResult.Success))
            {
                return null;
            }

            return configuration;
        }
    }
}

[RequireFeatures("OrchardCore.Deployment", OpenIdConstants.Features.Server)]
public sealed class ServerDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<OpenIdServerDeploymentSource, OpenIdServerDeploymentStep, OpenIdServerDeploymentStepDriver>();
    }
}

[Feature(OpenIdConstants.Features.Validation)]
public sealed class ValidationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOpenIddict()
            .AddValidation(options =>
            {
                options.UseAspNetCore();
                options.UseDataProtection();
                options.UseSystemNetHttp();
            });

        services.TryAddSingleton<IOpenIdValidationService, OpenIdValidationService>();

        // Note: the following services are registered using TryAddEnumerable to prevent duplicate registrations.
        services.TryAddEnumerable(new[]
        {
            ServiceDescriptor.Scoped<IDisplayDriver<OpenIdValidationSettings>, OpenIdValidationSettingsDisplayDriver>(),
        });

        services.AddRecipeExecutionStep<OpenIdValidationSettingsStep>();

        // Note: the OpenIddict ASP.NET host adds an authentication options initializer that takes care of
        // registering the validation handler. Yet, it MUST NOT be registered at this stage as it is
        // lazily registered by OpenIdValidationConfiguration only after checking the OpenID validation
        // settings are valid and can be safely used in this tenant without causing runtime exceptions.
        // To prevent that, the initializer is manually removed from the services collection of the tenant.
        services.RemoveAll<IConfigureOptions<AuthenticationOptions>, OpenIddictValidationAspNetCoreConfiguration>();

        services.TryAddEnumerable(new[]
        {
            ServiceDescriptor.Singleton<IConfigureOptions<AuthenticationOptions>, OpenIdValidationConfiguration>(),
            ServiceDescriptor.Singleton<IConfigureOptions<ApiAuthorizationOptions>, OpenIdValidationConfiguration>(),
            ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictValidationOptions>, OpenIdValidationConfiguration>(),
            ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictValidationDataProtectionOptions>, OpenIdValidationConfiguration>()
        });
    }
}

[RequireFeatures("OrchardCore.Deployment", OpenIdConstants.Features.Validation)]
public sealed class ValidationDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<OpenIdValidationDeploymentSource, OpenIdValidationDeploymentStep, OpenIdValidationDeploymentStepDriver>();
    }
}

internal static class OpenIdServiceCollectionExtensions
{
    public static IServiceCollection RemoveAll(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        Debug.Assert(services != null, "The services collection shouldn't be null.");
        Debug.Assert(serviceType != null, "The service type shouldn't be null.");
        Debug.Assert(implementationType != null, "The implementation type shouldn't be null.");

        for (var index = services.Count - 1; index >= 0; index--)
        {
            var descriptor = services[index];
            if (descriptor.ServiceType == serviceType && descriptor.GetImplementationType() == implementationType)
            {
                services.RemoveAt(index);
            }
        }

        return services;
    }

    public static IServiceCollection RemoveAll<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : TService
        => services.RemoveAll(typeof(TService), typeof(TImplementation));
}
