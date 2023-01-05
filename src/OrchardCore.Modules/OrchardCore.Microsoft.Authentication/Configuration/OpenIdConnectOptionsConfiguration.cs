using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using OrchardCore.Microsoft.Authentication.Services;
using MicrosoftIdentityDefaults = Microsoft.Identity.Web.Constants;

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    internal class OpenIdConnectOptionsConfiguration : IConfigureNamedOptions<OpenIdConnectOptions>
    {
        private readonly IOptionsMonitor<MicrosoftIdentityOptions> _azureADOptions;
        private readonly IAzureADService _azureADService;

        public OpenIdConnectOptionsConfiguration(IOptionsMonitor<MicrosoftIdentityOptions> azureADOptions, IAzureADService azureADService)
        {
            _azureADOptions = azureADOptions;
            _azureADService = azureADService;
        }

        public void Configure(string name, OpenIdConnectOptions options)
        {
            if (name != OpenIdConnectDefaults.AuthenticationScheme)
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

            var settings = _azureADService.GetSettingsAsync().GetAwaiter().GetResult();
            options.SaveTokens = settings.SaveTokens;

        }

        public void Configure(OpenIdConnectOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
