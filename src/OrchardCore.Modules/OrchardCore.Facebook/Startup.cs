using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using OrchardCore.Environment.Navigation;
using OrchardCore.Modules;
using OrchardCore.Facebook.Configuration;
using OrchardCore.Facebook.Drivers;
using OrchardCore.Facebook.Recipes;
using OrchardCore.Facebook.Services;
using OrchardCore.Recipes;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using YesSql.Indexes;

namespace OrchardCore.Facebook
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }
    }

    [Feature(FacebookConstants.Features.Core)]
    public class CoreStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFacebookCoreService, FacebookCoreService>();
            services.AddScoped<IDisplayDriver<ISite>, FacebookCoreSettingsDisplayDriver>();
        }
    }


    [Feature(FacebookConstants.Features.Login)]
    public class LoginStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFacebookLoginService, FacebookLoginService>();
            services.AddScoped<IDisplayDriver<ISite>, FacebookLoginSettingsDisplayDriver>();

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


}
