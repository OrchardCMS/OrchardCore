using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    internal class CookieOptionsConfiguration : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly IOptionsMonitor<AzureADOptions> _azureADOptions;

        public CookieOptionsConfiguration(IOptionsMonitor<AzureADOptions> azureADOptions)
        {
            _azureADOptions = azureADOptions;
        }

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            if (name != "Identity.External")
            {
                return;
            }

            options.LoginPath = $"/AzureAD/Account/SignIn/{AzureADDefaults.AuthenticationScheme}";
            options.LogoutPath = $"/AzureAD/Account/SignOut/{AzureADDefaults.AuthenticationScheme}";
            options.AccessDeniedPath = "/AzureAD/Account/AccessDenied";
        }

        public void Configure(CookieAuthenticationOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
