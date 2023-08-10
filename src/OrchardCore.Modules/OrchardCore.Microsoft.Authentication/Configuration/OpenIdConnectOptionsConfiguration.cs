using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using OrchardCore.Microsoft.Authentication.Services;
using MicrosoftIdentityDefaults = Microsoft.Identity.Web.Constants;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    internal class OpenIdConnectOptionsConfiguration : IConfigureNamedOptions<OpenIdConnectOptions>
    {
        private readonly IOptionsMonitor<MicrosoftIdentityOptions> _azureADOptions;
        private readonly AzureADSettings _azureADSettings;

        public OpenIdConnectOptionsConfiguration(
            IOptionsMonitor<MicrosoftIdentityOptions> azureADOptions,
            IOptions<AzureADSettings> azureADSettings)
        {
            _azureADOptions = azureADOptions;
            _azureADSettings = azureADSettings.Value;
        }

        public void Configure(string name, OpenIdConnectOptions options)
        {
            if (name != AzureADOptionsConfiguration.AzureAdOpenIdConnectScheme)
            {
                return;
            }

            var azureADOptions = _azureADOptions.Get(MicrosoftIdentityDefaults.AzureAd);

            options.ClientId = azureADOptions.ClientId;
            options.ClientSecret = azureADOptions.ClientSecret;
            options.Authority = new Uri(new Uri(azureADOptions.Instance), azureADOptions.TenantId).ToString();
            options.CallbackPath = azureADOptions.CallbackPath.HasValue ? azureADOptions.CallbackPath : options.CallbackPath;
            options.SignedOutCallbackPath = azureADOptions.SignedOutCallbackPath.HasValue ? azureADOptions.SignedOutCallbackPath : options.SignedOutCallbackPath;
            options.SignInScheme = "Identity.External";
            options.UseTokenLifetime = true;
            options.SaveTokens = _azureADSettings.SaveTokens;

        }

        public void Configure(OpenIdConnectOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
