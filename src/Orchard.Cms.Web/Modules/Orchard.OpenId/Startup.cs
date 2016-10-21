using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Data.Migration;
using Orchard.Environment.Navigation;
using Orchard.Environment.Shell;
using Orchard.OpenId.Indexes;
using Orchard.OpenId.Models;
using Orchard.OpenId.Services;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Users.Models;
using System;
using System.IO;
using YesSql.Core.Indexes;

namespace Orchard.OpenId
{
    public class Startup : StartupBase
    {
        private const string CertificateFileName = "Certificate.pfx";

        private readonly string _certificateFullPath;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly string _tenantUrlPrefix;
        private readonly string _tenantName;

        public Startup(
            ShellSettings shellSettings,
            IOptions<ShellOptions> options,
            IHostingEnvironment environment,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider)
        {
            _tenantName = shellSettings.Name;
            _tenantUrlPrefix = shellSettings.RequestUrlPrefix;
            _dataProtectionProvider = dataProtectionProvider.CreateProtector(_tenantName);
            _certificateFullPath = Path.Combine(
                environment.ContentRootPath,
                options.Value.ShellsRootContainerName,
                options.Value.ShellsContainerName,
                shellSettings.Name,
                CertificateFileName);
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var tenantUrl = serviceProvider.GetRequiredService<ISiteService>().GetSiteSettingsAsync().Result.BaseUrl + _tenantUrlPrefix;

            builder.UseOpenIddict();

            builder.UseJwtBearerAuthentication(new JwtBearerOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
#if DEBUG
                RequireHttpsMetadata = false,
#endif
                Audience = tenantUrl,
                Authority = tenantUrl
            });
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IIndexProvider, OpenIdApplicationIndexProvider>();
            services.AddScoped<IIndexProvider, OpenIdTokenIndexProvider>();
            services.AddScoped<INavigationProvider, AdminMenu>();

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
                .UseJsonWebTokens()

                .RequireClientIdentification()

                // Use the advanced ApplicationCanDisplayErrors = true option to allow AccessController
                // to handle OpenID Connect error responses and render them in a custom error view.
                .Configure(options => options.ApplicationCanDisplayErrors = true);

#if DEBUG
            builder.DisableHttpsRequirement()
                .AddEphemeralSigningKey();
#else
            // On production, using a X.509 certificate stored in the machine store is recommended.
            // You can generate a self-signed certificate using Pluralsight's self-cert utility:
            // https://s3.amazonaws.com/pluralsight-free/keith-brown/samples/SelfCert.zip

            if (File.Exists(_certificateFullPath))
            {
                using (FileStream stream = File.Open(_certificateFullPath, FileMode.Open))
                {
                    //I need a password for opening the certificate ¿should I store this password in clear text on db? in the mean time I use a fixed password
                    builder.AddSigningCertificate(stream, "password");
                }
            }
#endif
        }
    }
}
