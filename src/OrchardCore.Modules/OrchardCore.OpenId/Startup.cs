using System;
using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Mvc;
using OpenIddict.Server;
using OpenIddict.Validation;
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
using System.Linq;
using OrchardCore.Entities;

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
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, OpenIdClientConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<OpenIdConnectOptions>, OpenIdClientConfiguration>(),

                // Built-in initializers:
                ServiceDescriptor
                    .Transient<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>()
            });
        }
    }

    [Feature(OpenIdConstants.Features.Server)]
    public class ServerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IOpenIdServerService, OpenIdServerService>();
            services.AddScoped<IDisplayDriver<ISite>, OpenIdServerSettingsDisplayDriver>();

            services.TryAddScoped<IOpenIddictServerEventService, OpenIddictServerEventService>();
            services.TryAddScoped<OpenIddictServerHandler>();
            services.TryAddScoped<OpenIddictServerProvider>();

            services.Configure<MvcOptions>(options =>
                options.ModelBinderProviders.Insert(0, new OpenIddictMvcBinderProvider()));

            // Register the options initializers required by OpenIddict and the JWT handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, OpenIdServerConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<JwtBearerOptions>, OpenIdServerConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<OpenIddictMvcOptions>, OpenIdServerConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<OpenIddictServerOptions>, OpenIdServerConfiguration>(),
                ServiceDescriptor
                    .Transient<IConfigureOptions<OpenIddictValidationOptions>, OpenIdServerConfiguration>(),

                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>(),
                ServiceDescriptor
                    .Transient<IPostConfigureOptions<OpenIddictServerOptions>, OpenIddictServerInitializer>(),
                ServiceDescriptor
                    .Transient<IPostConfigureOptions<OpenIddictServerOptions>, OpenIdConnectServerInitializer>(),
                ServiceDescriptor
                    .Transient<IPostConfigureOptions<OpenIddictValidationOptions>, OpenIddictValidationInitializer>(),
                ServiceDescriptor
                    .Transient<IPostConfigureOptions<OpenIddictValidationOptions>, OAuthValidationInitializer>()
            });

            // Disabling same-site is required for OpenID's module prompt=none support to work correctly.
            services.ConfigureApplicationCookie(options => options.Cookie.SameSite = SameSiteMode.None);
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var servic = serviceProvider.GetRequiredService<IOpenIdServerService>();
            var service = serviceProvider.GetRequiredService<ISiteService>();
            
            var settings = service.GetSiteSettingsAsync()
                .GetAwaiter()
                .GetResult()
                .As<Settings.OpenIdServerSettings>();

            if (settings != null)
            {
                var validationResult = servic.ValidateSettingsAsync(settings)
                    .GetAwaiter()
                    .GetResult();

                if (validationResult.Length == 0)
                {
                    var openIddictServerOptions = serviceProvider
                        .GetRequiredService<IOptionsMonitor<OpenIddictServerOptions>>()
                        .Get(OpenIddictServerDefaults.AuthenticationScheme);

                    if (openIddictServerOptions.AuthorizationEndpointPath != PathString.Empty)
                    {
                        routes.MapAreaRoute(
                            name: "Access.AuthorizeGet",
                            areaName: OpenIdConstants.Features.Core,
                            template: openIddictServerOptions.AuthorizationEndpointPath.Value,
                            defaults: new {controller = "Access", action = "Authorize"}
                        );

                        routes.MapAreaRoute(
                            name: "Access.Authorize",
                            areaName: OpenIdConstants.Features.Core,
                            template: openIddictServerOptions.AuthorizationEndpointPath.Value,
                            defaults: new {controller = "Access", action = "Accept"}
                        );

                        routes.MapAreaRoute(
                            name: "Access.Deny",
                            areaName: OpenIdConstants.Features.Core,
                            template: openIddictServerOptions.AuthorizationEndpointPath.Value,
                            defaults: new {controller = "Access", action = "Deny"}
                        );

                    }

                    if (openIddictServerOptions.TokenEndpointPath != PathString.Empty)
                    {
                        routes.MapAreaRoute(
                            name: "Access.Token",
                            areaName: OpenIdConstants.Features.Core,
                            template: openIddictServerOptions.TokenEndpointPath.Value,
                            defaults: new {controller = "Access", action = "Token"}
                        );
                    }

                    if (openIddictServerOptions.LogoutEndpointPath != PathString.Empty)
                    {
                        routes.MapAreaRoute(
                            name: "Access.Logout",
                            areaName: OpenIdConstants.Features.Core,
                            template: openIddictServerOptions.LogoutEndpointPath.Value,
                            defaults: new {controller = "Access", action = "Logout"}
                        );
                    }

                    if (openIddictServerOptions.UserinfoEndpointPath != PathString.Empty)
                    {
                        routes.MapAreaRoute(
                            name: "UserInfo.Me",
                            areaName: OpenIdConstants.Features.Core,
                            template: openIddictServerOptions.UserinfoEndpointPath.Value,
                            defaults: new {controller = "UserInfo", action = "Me"}
                        );
                    }
                }
            }
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

            services.TryAddScoped(provider =>
                (IOpenIdApplicationManager) provider.GetRequiredService<IOpenIddictApplicationManager>());
            services.TryAddScoped(provider =>
                (IOpenIdAuthorizationManager) provider.GetRequiredService<IOpenIddictAuthorizationManager>());
            services.TryAddScoped(provider =>
                (IOpenIdScopeManager) provider.GetRequiredService<IOpenIddictScopeManager>());
            services.TryAddScoped(provider =>
                (IOpenIdTokenManager) provider.GetRequiredService<IOpenIddictTokenManager>());

            services.AddSingleton<IIndexProvider, OpenIdApplicationIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdAuthorizationIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdScopeIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdTokenIndexProvider>();
            services.AddSingleton<IBackgroundTask, OpenIdBackgroundTask>();

            services.AddScoped<IRoleRemovedEventHandler, OpenIdApplicationRoleRemovedEventHandler>();

            services.AddScoped<IDataMigration, OpenIdMigrations>();

            services.AddRecipeExecutionStep<OpenIdServerSettingsStep>();
            services.AddRecipeExecutionStep<OpenIdApplicationStep>();
        }
    }

    [Feature(OpenIdConstants.Features.Validation)]
    public class ValidationStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IOpenIdValidationService, OpenIdValidationService>();
            services.AddScoped<IDisplayDriver<ISite>, OpenIdValidationSettingsDisplayDriver>();

            services.TryAddScoped<IOpenIddictValidationEventService, OpenIddictValidationEventService>();
            services.TryAddScoped<OpenIddictValidationHandler>();
            services.TryAddScoped<OpenIddictValidationProvider>();

            // Register the options initializers required by OpenIddict and the JWT handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, OpenIdValidationConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<JwtBearerOptions>, OpenIdValidationConfiguration>(),
                ServiceDescriptor
                    .Transient<IConfigureOptions<OpenIddictValidationOptions>, OpenIdValidationConfiguration>(),

                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>(),
                ServiceDescriptor
                    .Transient<IPostConfigureOptions<OpenIddictValidationOptions>, OpenIddictValidationInitializer>(),
                ServiceDescriptor
                    .Transient<IPostConfigureOptions<OpenIddictValidationOptions>, OAuthValidationInitializer>()
            });
        }
    }
}