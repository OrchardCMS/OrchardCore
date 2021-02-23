using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using OrchardCore.Microsoft.Authentication.Services;

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    internal class OpenIdConnectOptionsConfiguration : IConfigureNamedOptions<OpenIdConnectOptions>
    {
        private readonly IOptionsMonitor<AzureADOptions> _azureADOptions;
        private readonly IAzureADService _azureADService;

        public OpenIdConnectOptionsConfiguration(IOptionsMonitor<AzureADOptions> azureADOptions, IAzureADService azureADService)
        {
            _azureADOptions = azureADOptions;
            _azureADService = azureADService;
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

            var settings = _azureADService.GetSettingsAsync().GetAwaiter().GetResult();
            options.SaveTokens = settings.SaveTokens;

        }

        public void Configure(OpenIdConnectOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
