using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict;
using Orchard.Data.Migration;
using Orchard.Environment.Navigation;
using Orchard.Environment.Shell;
using Orchard.OpenId.Drivers;
using Orchard.OpenId.Indexes;
using Orchard.OpenId.Models;
using Orchard.OpenId.Recipes;
using Orchard.OpenId.Services;
using Orchard.OpenId.Settings;
using Orchard.Recipes;
using Orchard.Security.Permissions;
using Orchard.Settings.Services;
using YesSql.Indexes;

namespace Orchard.OpenId
{
    public class Startup : StartupBase
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<Startup> _logger;

        public Startup(
            ShellSettings shellSettings,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<Startup> logger)
        {
            _dataProtectionProvider = dataProtectionProvider.CreateProtector(shellSettings.Name);
            _logger = logger;
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var openIdService = serviceProvider.GetService<IOpenIdService>();
            var settings = openIdService.GetOpenIdSettingsAsync().GetAwaiter().GetResult();
            if (!openIdService.IsValidOpenIdSettings(settings))
            {
                _logger.LogWarning("The OpenID Connect module is not correctly configured.");
                return;
            }

            builder.UseOpenIddict();

            switch (settings.AccessTokenFormat)
            {
                case OpenIdSettings.TokenFormat.JWT:
                {
                    builder.UseJwtBearerAuthentication(new JwtBearerOptions
                    {
                        RequireHttpsMetadata = !settings.TestingModeEnabled,
                        Authority = settings.Authority,
                        TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidAudiences = settings.Audiences
                        }
                    });
                    break;
                }

                case OpenIdSettings.TokenFormat.Encrypted:
                {
                    builder.UseOAuthValidation(options =>
                    {
                        options.Audiences.UnionWith(settings.Audiences);
                        options.DataProtectionProvider = _dataProtectionProvider;
                    });
                    break;
                }

                default:
                {
                    Debug.Fail("An unsupported access token format was specified.");
                    break;
                }
            }

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
            services.AddScoped<IIndexProvider, OpenIdApplicationIndexProvider>();
            services.AddScoped<IIndexProvider, OpenIdTokenIndexProvider>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddScoped<ISiteSettingsDisplayDriver, OpenIdSiteSettingsDisplayDriver>();
            services.AddScoped<IOpenIdService, OpenIdService>();
            services.AddRecipeExecutionStep<OpenIdSettingsStep>();
            services.AddRecipeExecutionStep<OpenIdApplicationStep>();

            services.AddScoped<OpenIdApplicationIndexProvider>();
            services.AddScoped<OpenIdTokenIndexProvider>();

            services.AddScoped<OpenIdApplicationStore>();

            services.AddOpenIddict<OpenIdApplication, OpenIdAuthorization, OpenIdScope, OpenIdToken>(builder =>
            {
                builder.AddApplicationStore<OpenIdApplicationStore>()
                       .AddTokenStore<OpenIdTokenStore>();

                builder.UseDataProtectionProvider(_dataProtectionProvider);

                builder.RequireClientIdentification()
                       .EnableRequestCaching();

                builder.Configure(options => options.ApplicationCanDisplayErrors = true);
            });

            services.AddScoped<IConfigureOptions<OpenIddictOptions>, OpenIdConfiguration>();
        }
    }
}
