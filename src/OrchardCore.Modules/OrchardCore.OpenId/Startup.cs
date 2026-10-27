using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using OpenIddict.Client.DataProtection;
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
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Deployment;
using OrchardCore.OpenId.Drivers;
using OrchardCore.OpenId.Handlers;
using OrchardCore.OpenId.Migrations;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.Secrets.Drivers;
using OrchardCore.OpenId.Secrets.Services;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Services.Handlers;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.Tasks;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.RateLimits;
using OrchardCore.Recipes;
using OrchardCore.Secrets;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Users;
using OpenIdSecretsMigrations = OrchardCore.OpenId.Secrets.Migrations;

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
    }
}

[Feature(OpenIdConstants.Features.Client)]
public sealed class ClientStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<ClientAdminMenu>();
        services.AddScoped<IJSLocalizer, OpenIdJSLocalizer>();
        services.AddOpenIddict()
            .AddClient(options =>
            {
                options.UseAspNetCore();
                options.UseDataProtection();
                options.UseSystemNetHttp();
                options.UseWebProviders();
            });

        services.AddNavigationProvider<ClientAdminMenu>();
        services.TryAddSingleton<IOpenIdClientService, OpenIdClientService>();

        // Note: the following services are registered using TryAddEnumerable to prevent duplicate registrations.
        services.AddSiteDisplayDriver<OpenIdClientSettingsDisplayDriver>();

        services.AddRecipeExecutionStep<OpenIdClientSettingsStep>();

        // Note: the OpenIddict ASP.NET host adds an authentication options initializer that takes care of
        // registering the client ASP.NET Core handler. Yet, it MUST NOT be registered at this stage
        // as it is lazily registered by OpenIdClientConfiguration only after checking the OpenID client
        // settings are valid and can be safely used in this tenant without causing runtime exceptions.
        // To prevent that, the initializer is manually removed from the services collection of the tenant.
        services.RemoveAll<IConfigureOptions<AuthenticationOptions>, OpenIddictClientAspNetCoreConfiguration>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<AuthenticationOptions>, OpenIdClientConfiguration>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictClientOptions>, OpenIdClientConfiguration>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictClientAspNetCoreOptions>, OpenIdClientConfiguration>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictClientDataProtectionOptions>, OpenIdClientConfiguration>());

        services.TryAdd(OpenIdChallengeEventHandler.Descriptor.ServiceDescriptor);
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var settings = GetClientSettingsAsync().GetAwaiter().GetResult();
        if (settings is null)
        {
            return;
        }

        // Note: the redirection and post-logout redirection endpoints use the same default values
        // as the Microsoft ASP.NET Core OpenID Connect handler, for compatibility reasons.
        routes.MapAreaControllerRoute(
            name: "Callback.LogInCallback",
            areaName: typeof(Startup).Namespace,
            pattern: settings.CallbackPath ?? "signin-oidc",
            defaults: new { controller = "Callback", action = "LogInCallback" }
        );

        routes.MapAreaControllerRoute(
            name: "Callback.LogOutCallback",
            areaName: typeof(Startup).Namespace,
            pattern: settings.SignedOutCallbackPath ?? "signout-callback-oidc",
            defaults: new { controller = "Callback", action = "LogOutCallback" }
        );

        async Task<OpenIdClientSettings> GetClientSettingsAsync()
        {
            // Note: the OpenID client service is registered as a singleton service and thus can be
            // safely used with the non-scoped/root service provider available at this stage.
            var service = serviceProvider.GetRequiredService<IOpenIdClientService>();

            var configuration = await service.GetSettingsAsync();
            if ((await service.ValidateSettingsAsync(configuration)).Any(result => result != ValidationResult.Success))
            {
                return null;
            }

            return configuration;
        }
    }
}

[Feature(OpenIdConstants.Features.Client)]
[RequireFeatures("OrchardCore.Secrets")]
public sealed class ClientSecretsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<SecretMigration, OpenIdClientSecretMigrationDisplayDriver>();
    }
}

[Feature(OpenIdConstants.Features.Server)]
public sealed class ServerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<ServerAdminMenu>();

        services.AddOpenIddict()
            .AddServer(options =>
            {
                options.UseAspNetCore();
                options.UseDataProtection();
                options.AddEventHandler(PersistStoresHandler.Descriptor);
            });

        services.TryAddSingleton<IOpenIdServerService, OpenIdServerService>();

        services.AddDataMigration<DefaultScopesMigration>();
        services.AddDataMigration<PushedAuthorizationRequestsMigration>();

        services.AddDisplayDriver<OpenIdServerSettings, OpenIdServerSettingsDisplayDriver>();

        // Note: the following services are registered using TryAddEnumerable to prevent duplicate registrations.
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IRoleRemovedEventHandler, OpenIdApplicationRoleRemovedEventHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IBackgroundTask, OpenIdBackgroundTask>());

        services.AddRecipeExecutionStep<OpenIdServerSettingsStep>()
            .AddRecipeExecutionStep<OpenIdApplicationStep>()
            .AddRecipeExecutionStep<OpenIdScopeStep>();

        // Note: the OpenIddict ASP.NET host adds an authentication options initializer that takes care of
        // registering the server ASP.NET Core handler. Yet, it MUST NOT be registered at this stage
        // as it is lazily registered by OpenIdServerConfiguration only after checking the OpenID server
        // settings are valid and can be safely used in this tenant without causing runtime exceptions.
        // To prevent that, the initializer is manually removed from the services collection of the tenant.
        services.RemoveAll<IConfigureOptions<AuthenticationOptions>, OpenIddictServerAspNetCoreConfiguration>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<AuthenticationOptions>, OpenIdServerConfiguration>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictServerOptions>, OpenIdServerConfiguration>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictServerAspNetCoreOptions>, OpenIdServerConfiguration>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictServerDataProtectionOptions>, OpenIdServerConfiguration>());
    }

    public override async ValueTask ConfigureAsync(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var settings = await GetServerSettingsAsync();
        if (settings is null)
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

[Feature(OpenIdConstants.Features.Secrets)]
public sealed class ServerSecretsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<OpenIdServerSettings, OpenIdSecretSettingsDisplayDriver>();
        services.AddSingleton<IPostConfigureOptions<OpenIddictServerOptions>, OpenIdSecretsOptionsConfiguration>();
        services.AddDataMigration<OpenIdSecretsMigrations>();
    }
}

[Feature(OpenIdConstants.Features.Server)]
[RequireFeatures("OrchardCore.RateLimits")]
public sealed class ServerRateLimitsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Shares the budget of the login page, so it must use the same limit.
        var passwordAuthentication = RateLimitPartitionHelpers.CreateSlidingWindowPerIpPolicy(UserRateLimiterPolicyNames.PasswordAuthentication, 100);

        services.Configure<RateLimitsOptions>(options =>
            options.AddRouteRateLimit("Access.Token", HttpMethods.Post, context =>
            {
                // Only the password grant checks user credentials. The other grants exchange codes, refresh tokens or client secrets
                // that can't be guessed, and they are often sent for many users by a single server, so they must not share this budget.
                // The token request is extracted by the OpenID server handler, which runs in the authentication middleware.
                var request = context.GetOpenIddictServerRequest();

                return request is null || request.IsPasswordGrantType()
                    ? passwordAuthentication(context)
                    : RateLimitPartition.GetNoLimiter(UserRateLimiterPolicyNames.PasswordAuthentication);
            }));
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
        services.AddNavigationProvider<ValidationAdminMenu>();

        services.AddOpenIddict()
            .AddValidation(options =>
            {
                options.UseAspNetCore();
                options.UseDataProtection();
                options.UseSystemNetHttp();
            });

        services.TryAddSingleton<IOpenIdValidationService, OpenIdValidationService>();

        // Note: the following services are registered using TryAddEnumerable to prevent duplicate registrations.
        services.AddDisplayDriver<OpenIdValidationSettings, OpenIdValidationSettingsDisplayDriver>();

        services.AddRecipeExecutionStep<OpenIdValidationSettingsStep>();

        // Note: the OpenIddict ASP.NET host adds an authentication options initializer that takes care of
        // registering the validation handler. Yet, it MUST NOT be registered at this stage as it is
        // lazily registered by OpenIdValidationConfiguration only after checking the OpenID validation
        // settings are valid and can be safely used in this tenant without causing runtime exceptions.
        // To prevent that, the initializer is manually removed from the services collection of the tenant.
        services.RemoveAll<IConfigureOptions<AuthenticationOptions>, OpenIddictValidationAspNetCoreConfiguration>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<AuthenticationOptions>, OpenIdValidationConfiguration>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ApiAuthorizationOptions>, OpenIdValidationConfiguration>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictValidationOptions>, OpenIdValidationConfiguration>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictValidationDataProtectionOptions>, OpenIdValidationConfiguration>());
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

[Feature(OpenIdConstants.Features.Management)]
public sealed class ManagementStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<ManagementAdminMenu>();

        // Build the rows of the applications and scopes admin lists.
        services.AddDisplayDriver<OpenIdApplicationEntry, OpenIdApplicationDisplayDriver>();
        services.AddDisplayDriver<OpenIdScopeEntry, OpenIdScopeDisplayDriver>();

    }
}
