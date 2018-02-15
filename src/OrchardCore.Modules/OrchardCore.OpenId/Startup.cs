using System;
using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenIddict;
using OrchardCore.BackgroundTasks;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Navigation;
using OrchardCore.Modules;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.Drivers;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Services.Managers;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Services;
using OrchardCore.Recipes;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using YesSql.Indexes;

namespace OrchardCore.OpenId
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // Admin
            routes.MapAreaRoute(
                name: "AdminOpenId",
                areaName: "OrchardCore.OpenId",
                template: "Admin/OpenIdApps/{id?}/{action}",
                defaults: new { controller = "Admin" }
            );
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Note: only the core OpenIddict services (e.g managers and custom stores) are registered here.
            // The OpenIddict/JWT/validation handlers are lazily registered as active authentication handlers
            // depending on the OpenID settings. See the OpenIdConfiguration.cs file for more information.
            services.AddOpenIddict<IOpenIdApplication, IOpenIdAuthorization, IOpenIdScope, IOpenIdToken>()
                .AddApplicationManager<OpenIdApplicationManager>()
                .AddAuthorizationManager<OpenIdAuthorizationManager>()
                .AddScopeManager<OpenIdScopeManager>()
                .AddTokenManager<OpenIdTokenManager>();

            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddSingleton<IIndexProvider, OpenIdApplicationIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdAuthorizationIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdScopeIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdTokenIndexProvider>();
            services.AddSingleton<IBackgroundTask, OpenIdBackgroundTask>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddScoped<IDisplayDriver<ISite>, OpenIdSiteSettingsDisplayDriver>();
            services.AddSingleton<IOpenIdService, OpenIdService>();
            services.AddRecipeExecutionStep<OpenIdSettingsStep>();
            services.AddRecipeExecutionStep<OpenIdApplicationStep>();

            services.AddScoped<IRoleRemovedEventHandler, OpenIdApplicationRoleRemovedEventHandler>();

            services.TryAddScoped<OpenIdApplicationManager>();
            services.TryAddScoped<OpenIdAuthorizationManager>();
            services.TryAddScoped<OpenIdScopeManager>();
            services.TryAddScoped<OpenIdTokenManager>();

            // If no store was explicitly registered at the host level, add the default YesSql-based stores.
            // They can be later replaced or overriden by a module like the Entity Framework Core module.
            services.TryAddScoped<IOpenIdApplicationStore, OpenIdApplicationStore>();
            services.TryAddScoped<IOpenIdAuthorizationStore, OpenIdAuthorizationStore>();
            services.TryAddScoped<IOpenIdScopeStore, OpenIdScopeStore>();
            services.TryAddScoped<IOpenIdTokenStore, OpenIdTokenStore>();

            services.TryAddScoped<OpenIddictProvider<IOpenIdApplication, IOpenIdAuthorization, IOpenIdScope, IOpenIdToken>>();

            // Register the options initializers required by OpenIddict,
            // the JWT handler and the aspnet-contrib validation handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, OpenIdConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<OpenIddictOptions>, OpenIdConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<JwtBearerOptions>, OpenIdConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<OAuthValidationOptions>, OpenIdConfiguration>(),

                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>(),
                ServiceDescriptor.Transient<IPostConfigureOptions<OAuthValidationOptions>, OAuthValidationInitializer>(),
                ServiceDescriptor.Transient<IPostConfigureOptions<OpenIddictOptions>, OpenIddictInitializer>(),
                ServiceDescriptor.Transient<IPostConfigureOptions<OpenIddictOptions>, OpenIdConnectServerInitializer>()
            });

            // Disabling same-site is required for OpenID's module prompt=none support to work correctly.
            services.ConfigureApplicationCookie(options => options.Cookie.SameSite = SameSiteMode.None);
        }
    }
}
