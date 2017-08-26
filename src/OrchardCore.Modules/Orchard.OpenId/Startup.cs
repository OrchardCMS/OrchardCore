using System;
using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenIddict;
using Orchard.Data.Migration;
using Orchard.DisplayManagement.Handlers;
using Orchard.Environment.Navigation;
using Orchard.OpenId.Drivers;
using Orchard.OpenId.Indexes;
using Orchard.OpenId.Models;
using Orchard.OpenId.Recipes;
using Orchard.OpenId.Services;
using Orchard.Recipes;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Settings;
using YesSql.Indexes;

namespace Orchard.OpenId
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // Admin
            routes.MapAreaRoute(
                name: "AdminOpenId",
                areaName: "Orchard.OpenId",
                template: "Admin/OpenIdApps/{id?}/{action}",
                defaults: new { controller = "Admin" }
            );
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddSingleton<IIndexProvider, OpenIdApplicationIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdApplicationByRoleNameIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdTokenIndexProvider>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddScoped<IDisplayDriver<ISite>, OpenIdSiteSettingsDisplayDriver>();
            services.AddSingleton<IOpenIdService, OpenIdService>();
            services.AddRecipeExecutionStep<OpenIdSettingsStep>();
            services.AddRecipeExecutionStep<OpenIdApplicationStep>();

            services.AddScoped<OpenIdApplicationIndexProvider>();
            services.AddScoped<OpenIdTokenIndexProvider>();

            services.AddScoped<OpenIdApplicationStore>();

            services.AddScoped<IRoleRemovedEventHandler, OpenIdApplicationRoleRemovedEventHandler>();

            // Note: only the core OpenIddict services (e.g managers and custom stores) are registered here.
            // The OpenIddict/JWT/validation handlers are lazily registered as active authentication handlers
            // depending on the OpenID settings. See the OpenIdConfiguration.cs file for more information.
            services.AddOpenIddict<OpenIdApplication, OpenIdAuthorization, OpenIdScope, OpenIdToken>()
                .AddApplicationStore<OpenIdApplicationStore>()
                .AddAuthorizationStore<OpenIdAuthorizationStore>()
                .AddScopeStore<OpenIdScopeStore>()
                .AddTokenStore<OpenIdTokenStore>();

            // Register the OpenIddict handler/provider in the DI container.
            services.TryAddScoped<OpenIddictHandler>();
            services.TryAddScoped<OpenIddictProvider<OpenIdApplication, OpenIdAuthorization, OpenIdScope, OpenIdToken>>();

            // Register the options initializers required by OpenIddict,
            // the JWT handler and the aspnet-contrib validation handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Singleton<IConfigureOptions<AuthenticationOptions>, OpenIdConfiguration>(),
                ServiceDescriptor.Singleton<IConfigureOptions<OpenIddictOptions>, OpenIdConfiguration>(),
                ServiceDescriptor.Singleton<IConfigureOptions<JwtBearerOptions>, OpenIdConfiguration>(),
                ServiceDescriptor.Singleton<IConfigureOptions<OAuthValidationOptions>, OpenIdConfiguration>(),

                // Built-in initializers:
                ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>(),
                ServiceDescriptor.Singleton<IPostConfigureOptions<OAuthValidationOptions>, OAuthValidationInitializer>(),
                ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIddictOptions>, OpenIddictInitializer>(),
                ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIddictOptions>, OpenIdConnectServerInitializer>()
            });
        }
    }
}
