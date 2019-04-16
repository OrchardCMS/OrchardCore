using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Facebook.Drivers;
using OrchardCore.Facebook.Filters;
using OrchardCore.Facebook.Login.Configuration;
using OrchardCore.Facebook.Login.Drivers;
using OrchardCore.Facebook.Login.Services;
using OrchardCore.Facebook.Services;
using OrchardCore.Facebook.Widgets;
using OrchardCore.Facebook.Widgets.Drivers;
using OrchardCore.Facebook.Widgets.Handlers;
using OrchardCore.Facebook.Widgets.Models;
using OrchardCore.Facebook.Widgets.Settings;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Facebook
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddSingleton<IFacebookService, FacebookService>();
            services.AddScoped<IDisplayDriver<ISite>, FacebookSettingsDisplayDriver>();

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(FBInitFilter));
            });

        }
    }

    [Feature(FacebookConstants.Features.Login)]
    public class LoginStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFacebookLoginService, FacebookLoginService>();
            services.AddScoped<IDisplayDriver<ISite>, FacebookLoginSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenuLogin>();

            // Register the options initializers required by the OpenID Connect client handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, FacebookLoginConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<FacebookOptions>, FacebookLoginConfiguration>(),

                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<FacebookOptions>, OAuthPostConfigureOptions<FacebookOptions,FacebookHandler>>()
            });
        }
    }

    [Feature(FacebookConstants.Features.Widgets)]
    public class WidgetsStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentPartDisplayDriver, FacebookPluginPartDisplayDriver>();
            services.AddSingleton<ContentPart, FacebookPluginPart>();
            services.AddScoped<IContentPartHandler, FacebookPluginPartHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, FacebookPluginPartSettingsDisplayDriver>();

            //services.AddScoped<IResourceManifestProvider, ResourceManifest>();
            services.AddScoped<IDataMigration, WidgetMigrations>();

        }
    }

}
