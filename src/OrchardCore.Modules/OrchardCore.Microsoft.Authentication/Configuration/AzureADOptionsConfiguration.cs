using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using OrchardCore.Microsoft.Authentication.Settings;
using MicrosoftIdentityDefaults = Microsoft.Identity.Web.Constants;


namespace OrchardCore.Microsoft.Authentication.Configuration
{
    public class AzureADOptionsConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<PolicySchemeOptions>,
        IConfigureNamedOptions<MicrosoftIdentityOptions>
    {
        public const string AzureAdOpenIdConnectScheme = MicrosoftIdentityDefaults.AzureAd + OpenIdConnectDefaults.AuthenticationScheme;

        private readonly AzureADSettings _azureADSettings;

        public AzureADOptionsConfiguration(IOptions<AzureADSettings> azureADSettings)
        {
            _azureADSettings = azureADSettings.Value;
        }

        public void Configure(AuthenticationOptions options)
        {
            var settings = _azureADSettings;
            if (settings == null)
            {
                return;
            }

            // Register the OpenID Connect client handler in the authentication handlers collection.
            options.AddScheme(Constants.AzureAd, builder =>
            {
                builder.DisplayName = settings.DisplayName;
                builder.HandlerType = typeof(PolicySchemeHandler);
            });

            options.AddScheme(AzureAdOpenIdConnectScheme, builder =>
            {
                builder.HandlerType = typeof(OpenIdConnectHandler);
            });
        }

        public void Configure(string name, MicrosoftIdentityOptions options)
        {
            if (!String.Equals(name, MicrosoftIdentityDefaults.AzureAd))
            {
                return;
            }

            var loginSettings = _azureADSettings;
            if (loginSettings == null)
            {
                return;
            }

            options.ClientId = loginSettings.AppId;
            options.TenantId = loginSettings.TenantId;
            options.Instance = "https://login.microsoftonline.com/";

            if (loginSettings.CallbackPath.HasValue)
            {
                options.CallbackPath = loginSettings.CallbackPath;
            }
        }

        public void Configure(MicrosoftIdentityOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        public void Configure(string name, PolicySchemeOptions options)
        {
            if (!String.Equals(name, MicrosoftIdentityDefaults.AzureAd))
            {
                return;
            }

            options.ForwardDefault = "Identity.External";
            options.ForwardChallenge = AzureAdOpenIdConnectScheme;
        }
        public void Configure(PolicySchemeOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
