using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Mvc;
using OpenIddict.Server;
using OpenIddict.Server.Internal;
using OpenIddict.Validation;
using OpenIddict.Validation.Internal;
using OrchardCore.BackgroundTasks;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Modules;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Drivers;
using OrchardCore.OpenId.Handlers;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Services.Managers;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.Tasks;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Migrations;
using OrchardCore.OpenId.YesSql.Models;
using OrchardCore.OpenId.YesSql.Stores;
using OrchardCore.Recipes;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using YesSql.Indexes;

namespace OrchardCore.OpenId
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }
    }

    [Feature(OpenIdConstants.Features.Client)]
    public class ClientStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IOpenIdClientService, OpenIdClientService>();
            services.AddScoped<IDisplayDriver<ISite>, OpenIdClientSettingsDisplayDriver>();

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

    [Feature(OpenIdConstants.Features.Management)]
    public class ManagementStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Note: only the core OpenIddict services (e.g managers and custom stores) are registered here.
            // The OpenIddict/JWT/validation handlers are lazily registered as active authentication handlers
            // depending on the OpenID settings by OpenIdServerConfiguration and OpenIdValidationConfiguration.
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.ReplaceApplicationManager(typeof(OpenIdApplicationManager<>))
                           .ReplaceAuthorizationManager(typeof(OpenIdAuthorizationManager<>))
                           .ReplaceScopeManager(typeof(OpenIdScopeManager<>))
                           .ReplaceTokenManager(typeof(OpenIdTokenManager<>));

                    options.AddApplicationStore(typeof(OpenIdApplicationStore<>))
                           .AddAuthorizationStore(typeof(OpenIdAuthorizationStore<>))
                           .AddScopeStore(typeof(OpenIdScopeStore<>))
                           .AddTokenStore(typeof(OpenIdTokenStore<>));

                    options.SetDefaultApplicationEntity<OpenIdApplication>()
                           .SetDefaultAuthorizationEntity<OpenIdAuthorization>()
                           .SetDefaultScopeEntity<OpenIdScope>()
                           .SetDefaultTokenEntity<OpenIdToken>();
                });

            services.TryAddScoped(provider => (IOpenIdApplicationManager) provider.GetRequiredService<IOpenIddictApplicationManager>());
            services.TryAddScoped(provider => (IOpenIdAuthorizationManager) provider.GetRequiredService<IOpenIddictAuthorizationManager>());
            services.TryAddScoped(provider => (IOpenIdScopeManager) provider.GetRequiredService<IOpenIddictScopeManager>());
            services.TryAddScoped(provider => (IOpenIdTokenManager) provider.GetRequiredService<IOpenIddictTokenManager>());

            services.AddSingleton<IIndexProvider, OpenIdApplicationIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdAuthorizationIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdScopeIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdTokenIndexProvider>();

            services.AddScoped<IRoleRemovedEventHandler, OpenIdApplicationRoleRemovedEventHandler>();

            services.AddScoped<IDataMigration, OpenIdMigrations>();
        }
    }

    [Feature(OpenIdConstants.Features.Server)]
    public class ServerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IOpenIdServerService, OpenIdServerService>();
            services.TryAddTransient<JwtBearerHandler>();
            services.AddScoped<IDisplayDriver<ISite>, OpenIdServerSettingsDisplayDriver>();
            services.AddSingleton<IBackgroundTask, OpenIdBackgroundTask>();

            services.AddRecipeExecutionStep<OpenIdServerSettingsStep>();
            services.AddRecipeExecutionStep<OpenIdApplicationStep>();

            // Note: both the OpenIddict server and validation services are registered for the
            // server feature as token validation may be required for the userinfo endpoint.
            services.AddOpenIddict()
                .AddServer(options => options.UseMvc())
                .AddValidation();

            // Note: the OpenIddict extensions add two authentication options initializers that take care of
            // registering the server and validation handlers. Yet, they MUST NOT be registered at this stage
            // as they are lazily registered by OpenIdServerConfiguration only after checking the OpenID server
            // and validation settings are valid and can be safely used in this tenant without causing exceptions.
            // To prevent that, the initializers are manually removed from the services collection of the tenant.
            services.RemoveAll<IConfigureOptions<AuthenticationOptions>, OpenIddictServerConfiguration>()
                    .RemoveAll<IConfigureOptions<AuthenticationOptions>, OpenIddictValidationConfiguration>();

            // Register the options initializers required by OpenIddict and the JWT handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Singleton<IConfigureOptions<AuthenticationOptions>, OpenIdServerConfiguration>(),
                ServiceDescriptor.Singleton<IConfigureOptions<JwtBearerOptions>, OpenIdServerConfiguration>(),
                ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictMvcOptions>, OpenIdServerConfiguration>(),
                ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictServerOptions>, OpenIdServerConfiguration>(),
                ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictValidationOptions>, OpenIdServerConfiguration>(),

                // Built-in initializers (note: the OpenIddict initializers are registered by AddServer()/AddValidation()).
                ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>()
            });

            // Disabling same-site is required for OpenID's module prompt=none support to work correctly.
            services.ConfigureApplicationCookie(options => options.Cookie.SameSite = SameSiteMode.None);
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
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

            var settings = GetServerSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            if (settings.AuthorizationEndpointPath.HasValue)
            {
                routes.MapAreaRoute(
                    name: "Access.Authorize",
                    areaName: OpenIdConstants.Features.Core,
                    template: settings.AuthorizationEndpointPath.Value,
                    defaults: new { controller = "Access", action = "Authorize" }
                );
            }

            if (settings.LogoutEndpointPath.HasValue)
            {
                routes.MapAreaRoute(
                    name: "Access.Logout",
                    areaName: OpenIdConstants.Features.Core,
                    template: settings.LogoutEndpointPath.Value,
                    defaults: new { controller = "Access", action = "Logout" }
                );
            }

            if (settings.TokenEndpointPath.HasValue)
            {
                routes.MapAreaRoute(
                    name: "Access.Token",
                    areaName: OpenIdConstants.Features.Core,
                    template: settings.TokenEndpointPath.Value,
                    defaults: new { controller = "Access", action = "Token" }
                );
            }

            if (settings.UserinfoEndpointPath.HasValue)
            {
                routes.MapAreaRoute(
                    name: "UserInfo.Me",
                    areaName: OpenIdConstants.Features.Core,
                    template: settings.UserinfoEndpointPath.Value,
                    defaults: new { controller = "UserInfo", action = "Me" }
                );
            }
        }
    }

    [Feature(OpenIdConstants.Features.Validation)]
    public class ValidationStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IOpenIdValidationService, OpenIdValidationService>();
            services.TryAddTransient<JwtBearerHandler>();
            services.AddScoped<IDisplayDriver<ISite>, OpenIdValidationSettingsDisplayDriver>();

            services.AddOpenIddict()
                .AddValidation();

            // Note: the OpenIddict extensions add an authentication options initializer that takes care of
            // registering the validation handler. Yet, it MUST NOT be registered at this stage as it is
            // lazily registered by OpenIdValidationConfiguration only after checking the OpenID validation
            // settings are valid and can be safely used in this tenant without causing exceptions.
            // To prevent that, the initializer is manually removed from the services collection of the tenant.
            services.RemoveAll<IConfigureOptions<AuthenticationOptions>, OpenIddictValidationConfiguration>();

            // Register the options initializers required by OpenIddict and the JWT handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Singleton<IConfigureOptions<AuthenticationOptions>, OpenIdValidationConfiguration>(),
                ServiceDescriptor.Singleton<IConfigureOptions<JwtBearerOptions>, OpenIdValidationConfiguration>(),
                ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictValidationOptions>, OpenIdValidationConfiguration>(),

                // Built-in initializers (note: the OpenIddict initializers are registered by AddValidation()).
                ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>()
            });
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
                if (descriptor.ServiceType == serviceType && descriptor.ImplementationType == implementationType)
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
}
