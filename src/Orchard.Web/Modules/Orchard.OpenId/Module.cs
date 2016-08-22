using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;
using Orchard.FileSystem.AppData;
using Orchard.OpenId.Indexes;
using Orchard.OpenId.Models;
using Orchard.OpenId.Services;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Users.Models;
using System;
using Microsoft.AspNetCore.DataProtection;

namespace Orchard.OpenId
{
    public class Startup : StartupBase
    {     
        private readonly string _certificateFullPath;
        private const string certificateFileName = "Certificate.pfx";
        private readonly string tenantUrlPrefix;
        private readonly string tenantName;
        public Startup(ShellSettings shellSettings, IAppDataFolder appDataFolder, ILoggerFactory loggerFactory)   
        {            
            tenantName = shellSettings.Name;
            tenantUrlPrefix = shellSettings.RequestUrlPrefix;
            _certificateFullPath = appDataFolder.Combine(shellSettings.Name, certificateFileName);
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var tenantUrl = serviceProvider.GetRequiredService<ISiteService>().GetSiteSettingsAsync().Result.BaseUrl + tenantUrlPrefix;

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

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<OpenIdApplicationIndexProvider>();
            serviceCollection.AddScoped<OpenIdTokenIndexProvider>();            
            serviceCollection.TryAddScoped<IOpenIdApplicationManager, OpenIdApplicationManager>();
            serviceCollection.TryAddScoped<IOpenIdApplicationStore, OpenIdApplicationStore>();
            var openIddictBuilder = serviceCollection.AddOpenIddict<User, Role, OpenIdApplication, OpenIdAuthorization, OpenIdScope, OpenIdToken>()
            .AddApplicationStore<OpenIdApplicationStore>()
            .AddTokenStore<OpenIdTokenStore>()
            .AddUserStore<OpenIdUserStore>()
            .AddUserManager<OpenIdUserManager>()
            .Configure(options => options.DataProtectionProvider = DataProtectionProvider.Create(tenantName))
            .UseJsonWebTokens()            
            .EnableTokenEndpoint("/Orchard.OpenId/Access/Token")
            .EnableAuthorizationEndpoint("/Orchard.OpenId/Access/Authorize")
            .EnableLogoutEndpoint("/Orchard.OpenId/Access/Logout")
            .EnableUserinfoEndpoint("/Orchard.OpenId/Access/Userinfo")
            .AllowPasswordFlow()
            .AllowClientCredentialsFlow()
            .AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow();
            
#if DEBUG
            openIddictBuilder.DisableHttpsRequirement()
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
                    openIddictBuilder.AddSigningCertificate(stream,"password");
                }                
            }
#endif
        }
    }
}
