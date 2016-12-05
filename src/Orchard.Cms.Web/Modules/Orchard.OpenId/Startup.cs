using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
using Orchard.Security;
using Orchard.Settings.Services;
using Orchard.SiteSettings;
using Orchard.Users.Models;
using System;
using YesSql.Core.Indexes;

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
            var siteSettingsGroupProvider = serviceProvider.GetService<SiteSettingsGroupProvider>();
            var t = serviceProvider.GetService<IStringLocalizer<Startup>>();
            siteSettingsGroupProvider.Add("open id", t["Open Id"]);

            var openIdService  = serviceProvider.GetService<IOpenIdService>();
            var openIdSettings = openIdService.GetOpenIdSettingsAsync().Result;
            if (!openIdService.IsValidOpenIdSettings(openIdSettings))
            {
                _logger.LogWarning("Orchard.OpenId module has invalid settings.");
                return;
            }
        
            builder.UseOpenIddict();

            if (openIdSettings.DefaultTokenFormat == OpenIdSettings.TokenFormat.JWT)
            {
                builder.UseJwtBearerAuthentication();
            }
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IIndexProvider, OpenIdApplicationIndexProvider>();
            services.AddScoped<IIndexProvider, OpenIdTokenIndexProvider>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddScoped<ISiteSettingsDisplayDriver, OpenIdSiteSettingsDisplayDriver>();
            services.AddScoped<IOpenIdService, OpenIdService>();
            services.AddRecipeExecutionStep<OpenIdSettingsStep>();

            services.AddScoped<OpenIdApplicationIndexProvider>();
            services.AddScoped<OpenIdTokenIndexProvider>();
            services.TryAddScoped<IOpenIdApplicationManager, OpenIdApplicationManager>();
            services.TryAddScoped<IOpenIdApplicationStore, OpenIdApplicationStore>();

            var builder = services.AddOpenIddict<User, Role, OpenIdApplication, OpenIdAuthorization, OpenIdScope, OpenIdToken>()
                .AddApplicationStore<OpenIdApplicationStore>()
                .AddTokenStore<OpenIdTokenStore>()
                .AddUserStore<OpenIdUserStore>()
                .AddUserManager<OpenIdUserManager>()
                .EnableAuthorizationEndpoint("/Orchard.OpenId/Access/Authorize")
                .EnableLogoutEndpoint("/Orchard.OpenId/Access/Logout")
                .EnableTokenEndpoint("/Orchard.OpenId/Access/Token")
                .EnableUserinfoEndpoint("/Orchard.OpenId/Access/Userinfo")
                .AllowPasswordFlow()
                .AllowClientCredentialsFlow()
                .AllowAuthorizationCodeFlow()
                .AllowRefreshTokenFlow()
                .UseDataProtectionProvider(_dataProtectionProvider)
                .RequireClientIdentification()
                .Configure(options => options.ApplicationCanDisplayErrors = true);

            services.AddScoped<IConfigureOptions<OpenIddictOptions>, OpenIddictConfigureOptions>();
            services.AddScoped<IConfigureOptions<JwtBearerOptions>, JwtBearerConfigureOptions>();

        }
    }
}
