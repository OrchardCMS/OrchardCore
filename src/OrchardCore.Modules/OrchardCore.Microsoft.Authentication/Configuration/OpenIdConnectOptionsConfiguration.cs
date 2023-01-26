using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using OrchardCore.Microsoft.Authentication.Settings;

#pragma warning disable CS0618
// The net5.0 5.0.3 build obsoletes 'AzureADOptions' and 'AzureADDefaults', 'Microsoft.Identity.Web' should be used instead.
// The build warning is disabled temporarily until the code can be migrated.

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    internal class OpenIdConnectOptionsConfiguration : IConfigureNamedOptions<OpenIdConnectOptions>
    {
        private readonly IOptionsMonitor<AzureADOptions> _azureADOptions;
        private readonly AzureADSettings _azureADSettings;

        public OpenIdConnectOptionsConfiguration(
            IOptionsMonitor<AzureADOptions> azureADOptions,
            IOptions<AzureADSettings> azureADSettings)
        {
            _azureADOptions = azureADOptions;
            _azureADSettings = azureADSettings.Value;
        }

        public void Configure(string name, OpenIdConnectOptions options)
        {
            if (name != AzureADDefaults.OpenIdScheme)
            {
                return;
            }

            var azureADOptions = _azureADOptions.Get(AzureADDefaults.AuthenticationScheme);

            options.ClientId = azureADOptions.ClientId;
            options.ClientSecret = azureADOptions.ClientSecret;
            options.Authority = new Uri(new Uri(azureADOptions.Instance), azureADOptions.TenantId).ToString();
            options.CallbackPath = azureADOptions.CallbackPath ?? options.CallbackPath;
            options.SignedOutCallbackPath = azureADOptions.SignedOutCallbackPath ?? options.SignedOutCallbackPath;
            options.SignInScheme = "Identity.External";
            options.UseTokenLifetime = true;
            options.SaveTokens = _azureADSettings.SaveTokens;

        }

        public void Configure(OpenIdConnectOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}

// Restore the obsolete warning disabled above
#pragma warning restore CS0618

