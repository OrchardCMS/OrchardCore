using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
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
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Drivers;
using OrchardCore.OpenId.Handlers;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Services.Managers;
using OrchardCore.OpenId.Tasks;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Migrations;
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
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, OpenIdClientConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<OpenIdConnectOptions>, OpenIdClientConfiguration>(),

                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>()
            });
        }
    }

    [Feature(OpenIdConstants.Features.Server)]
    public class ServerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IOpenIdServerService, OpenIdServerService>();
            services.AddScoped<IDisplayDriver<ISite>, OpenIdServerSettingsDisplayDriver>();

            services.TryAddScoped<OpenIddictProvider<IOpenIdApplication, IOpenIdAuthorization, IOpenIdScope, IOpenIdToken>>();

            // Register the options initializers required by OpenIddict,
            // the JWT handler and the aspnet-contrib validation handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, OpenIdServerConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<OpenIddictOptions>, OpenIdServerConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<JwtBearerOptions>, OpenIdServerConfiguration>(),

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

    [Feature(OpenIdConstants.Features.Management)]
    public class ManagementStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Note: only the core OpenIddict services (e.g managers and custom stores) are registered here.
            // The OpenIddict/JWT/validation handlers are lazily registered as active authentication handlers
            // depending on the OpenID settings by OpenIdServerConfiguration and OpenIdValidationConfiguration.
            services.AddOpenIddict<IOpenIdApplication, IOpenIdAuthorization, IOpenIdScope, IOpenIdToken>()
                .AddApplicationManager<OpenIdApplicationManager>()
                .AddAuthorizationManager<OpenIdAuthorizationManager>()
                .AddScopeManager<OpenIdScopeManager>()
                .AddTokenManager<OpenIdTokenManager>();

            services.AddSingleton<IIndexProvider, OpenIdApplicationIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdAuthorizationIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdScopeIndexProvider>();
            services.AddSingleton<IIndexProvider, OpenIdTokenIndexProvider>();
            services.AddSingleton<IBackgroundTask, OpenIdBackgroundTask>();

            // If no store was explicitly registered at the host level, add the default YesSql-based stores.
            // They can be later replaced or overriden by a module like the Entity Framework Core module.
            services.TryAddScoped<IOpenIdApplicationStore, OpenIdApplicationStore>();
            services.TryAddScoped<IOpenIdAuthorizationStore, OpenIdAuthorizationStore>();
            services.TryAddScoped<IOpenIdScopeStore, OpenIdScopeStore>();
            services.TryAddScoped<IOpenIdTokenStore, OpenIdTokenStore>();

            services.AddScoped<IRoleRemovedEventHandler, OpenIdApplicationRoleRemovedEventHandler>();

            services.TryAddScoped<OpenIdApplicationManager>();
            services.TryAddScoped<OpenIdAuthorizationManager>();
            services.TryAddScoped<OpenIdScopeManager>();
            services.TryAddScoped<OpenIdTokenManager>();

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
            services.AddSingleton<IOpenIdValidationService, OpenIdValidationService>();
            services.AddScoped<IDisplayDriver<ISite>, OpenIdValidationSettingsDisplayDriver>();

            // Register the options initializers required by OpenIddict,
            // the JWT handler and the aspnet-contrib validation handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, OpenIdValidationConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<JwtBearerOptions>, OpenIdValidationConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<OAuthValidationOptions>, OpenIdValidationConfiguration>(),

                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>(),
                ServiceDescriptor.Transient<IPostConfigureOptions<OAuthValidationOptions>, OAuthValidationInitializer>(),
                ServiceDescriptor.Transient<IPostConfigureOptions<OpenIddictOptions>, OpenIddictInitializer>(),
                ServiceDescriptor.Transient<IPostConfigureOptions<OpenIddictOptions>, OpenIdConnectServerInitializer>()
            });
        }
    }
}
