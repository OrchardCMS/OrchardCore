using System;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    // Copyright (c) .NET Foundation. All rights reserved.
    // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.using Microsoft.AspNetCore.Authorization;

    internal class OpenIdConnectOptionsConfiguration : IConfigureNamedOptions<OpenIdConnectOptions>
    {
        private readonly IOptionsMonitor<AzureADOptions> _azureADOptions;

        public OpenIdConnectOptionsConfiguration(IOptionsMonitor<AzureADOptions> azureADOptions)
        {
            _azureADOptions = azureADOptions;
        }

        public void Configure(string name, OpenIdConnectOptions options)
        {
            var azureADScheme = AzureADDefaults.AuthenticationScheme;
            var azureADOptions = _azureADOptions.Get(azureADScheme);
            if (name != azureADScheme)
            {
                return;
            }

            options.ClientId = azureADOptions.ClientId;
            options.ClientSecret = azureADOptions.ClientSecret;
            options.Authority = new Uri(new Uri(azureADOptions.Instance), azureADOptions.TenantId).ToString();
            options.CallbackPath = azureADOptions.CallbackPath ?? options.CallbackPath;
            options.SignedOutCallbackPath = azureADOptions.SignedOutCallbackPath ?? options.SignedOutCallbackPath;
            options.SignInScheme = azureADOptions.CookieSchemeName;
            options.UseTokenLifetime = true;
        }

        public void Configure(OpenIdConnectOptions options)
        {
        }
    }
}
