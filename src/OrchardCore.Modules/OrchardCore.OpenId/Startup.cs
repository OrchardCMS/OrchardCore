using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
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

        services.TryAddSingleton<IOpenIdClientService, OpenIdClientService>();
        services.TryAddSingleton<IOpenIdClientRegistrationService, OpenIdClientRegistrationService>();
        services.TryAddSingleton<OpenIdClientRegistrationFactory>();
        services.TryAddSingleton<OpenIdClientRegistrationResolver>();

        // The registration of the site settings comes first, so it keeps its provider name over another provider.
        services.AddOpenIdClientRegistrationProvider<SiteOpenIdClientRegistrationProvider>();
        services.AddOpenIdClientRegistrationProvider<StoredOpenIdClientRegistrationProvider>();

        // Note: the following services are registered using TryAddEnumerable to prevent duplicate registrations.
        services.AddSiteDisplayDriver<OpenIdClientSettingsDisplayDriver>();
        services.AddDisplayDriver<OpenIdClientRegistration, OpenIdClientRegistrationDisplayDriver>();

        services.AddRecipeExecutionStep<OpenIdClientSettingsStep>();
        services.AddRecipeExecutionStep<OpenIdClientRegistrationsStep>();
        services.AddDeployment<OpenIdClientRegistrationsDeploymentSource, OpenIdClientRegistrationsDeploymentStep, OpenIdClientRegistrationsDeploymentStepDriver>();

        // Note: the OpenIddict ASP.NET host adds an authentication options initializer that takes care of
        // registering the client ASP.NET Core handler. Yet, it MUST NOT be registered at this stage
        // as it is lazily registered by OpenIdClientConfiguration only after checking a valid client
        // registration exists and can be safely used in this tenant without causing runtime exceptions.
        // To prevent that, the initializer is manually removed from the services collection of the tenant.
        services.RemoveAll<IConfigureOptions<AuthenticationOptions>, OpenIddictClientAspNetCoreConfiguration>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<AuthenticationOptions>, OpenIdClientConfiguration>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictClientOptions>, OpenIdClientConfiguration>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictClientAspNetCoreOptions>, OpenIdClientConfiguration>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictClientDataProtectionOptions>, OpenIdClientConfiguration>());

        services.TryAdd(OpenIdChallengeEventHandler.Descriptor.ServiceDescriptor);
    }

    public override async ValueTask ConfigureAsync(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        // Note: the resolver is registered as a singleton service and thus can be
        // safely used with the non-scoped/root service provider available at this stage.
        var registrations = await serviceProvider.GetRequiredService<OpenIdClientRegistrationResolver>().GetRegistrationsAsync();

        // The redirection and post-logout redirection endpoints of all the registrations are handled by the
        // callback controller. Several registrations can share an endpoint, as OpenIddict resolves the
        // registration of a callback from its state token.
        var index = 0;

        foreach (var uri in registrations
            .Where(registration => registration.RedirectUri is { IsAbsoluteUri: false })
            .Select(registration => registration.RedirectUri.OriginalString)
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            routes.MapAreaControllerRoute(
                name: $"OpenIdClient.LogInCallback.{index++}",
                areaName: typeof(Startup).Namespace,
                pattern: uri,
                defaults: new { controller = "Callback", action = "LogInCallback" }
            );
        }

        index = 0;

        foreach (var uri in registrations
            .Where(registration => registration.PostLogoutRedirectUri is { IsAbsoluteUri: false })
            .Select(registration => registration.PostLogoutRedirectUri.OriginalString)
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            routes.MapAreaControllerRoute(
                name: $"OpenIdClient.LogOutCallback.{index++}",
                areaName: typeof(Startup).Namespace,
                pattern: uri,
                defaults: new { controller = "Callback", action = "LogOutCallback" }
            );
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
    private const string TokenRouteName = "Access.Token";

    public override void ConfigureServices(IServiceCollection services)
    {
        // Shares the budget of the login page, so it must use the same limit.
        var passwordAuthentication = RateLimitPartitionHelpers.CreateSlidingWindowPerIpPolicy(UserRateLimiterPolicyNames.PasswordAuthentication, 100);

        services.Configure<RateLimitsOptions>(options =>
        {
            options.AddRouteRateLimit(TokenRouteName, HttpMethods.Post, context =>
            {
                // Only the password grant checks user credentials. The other grants exchange codes, refresh tokens or client secrets
                // that can't be guessed, and they are often sent for many users by a single server, so they must not share this budget.
                // The token request is extracted by the OpenID server handler, which runs in the authentication middleware.
                var request = context.GetOpenIddictServerRequest();

                return request is null || request.IsPasswordGrantType()
                    ? passwordAuthentication(context)
                    : RateLimitPartition.GetNoLimiter(UserRateLimiterPolicyNames.PasswordAuthentication);
            });

            options.AddRouteRejectionHandler(TokenRouteName, OnTokenRequestRejectedAsync);
        });
    }

    private static async ValueTask OnTokenRequestRejectedAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        // Lets the OpenID server handler return an OAuth 2.0 error response, which token clients understand, instead of an empty response.
        if (context.HttpContext.GetOpenIddictServerRequest() is null)
        {
            return;
        }

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        }

        // The 'slow_down' error is defined by the device authorization grant, but it's the error that best describes a rejected request.
        // The error description is not localized as OAuth 2.0 only allows select US-ASCII characters in it.
        var properties = new AuthenticationProperties(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.SlowDown,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Too many requests. Please retry later.",
        });

        await context.HttpContext.ForbidAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, properties);
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
