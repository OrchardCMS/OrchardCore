using OrchardCore.OpenIdConnect.Drivers;
using OrchardCore.OpenIdConnect.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Navigation;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using System;

namespace OrchardCore.OpenIdConnect
{
    public class Startup : StartupBase
    {
        private const string LoginPath = "Login";

        private ILogger<Startup> _logger;

        public Startup(ILogger<Startup> logger)
        {
            _logger = logger;
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // Configure the OWIN pipeline to use OpenID Connect auth.
            builder.UseAuthentication();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSecurity();

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, OpenIdConnectSettingsDisplayDriver>();
            services.AddSingleton<IOpenIdConnectService, OpenIdConnectService>();
            services.AddAuthentication().AddCookie();

            // Register the options initializers required by OpenIdConnect,
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, OpenIdConnectConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<OpenIdConnectOptions>, OpenIdConnectConfiguration>(),

                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>()
            });


            //options =>
            //{
            //    options.Authority = authority;
            //    options.ClientId = clientId;
            //    //options.SignedOutRedirectUri = _shellSettings.Configuration["OpenIdConnect.SignedOutRedirectUri"];
            //    //options.SignInScheme = _shellSettings.Configuration["OpenIdConnect.SignInScheme"];
            //    options.Scope.Add("email");
            //    options.Scope.Add("profile");
            //    options.GetClaimsFromUserInfoEndpoint = true;
            //    options.RequireHttpsMetadata = false;
            //    options.ResponseType = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectResponseType.IdToken;
            //    options.CallbackPath = "/signin-idsrv";
            //}

        }

        // Handle sign-in errors differently than generic errors.

    }
}
