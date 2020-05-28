/*
	OpenID Connect (standatd) is a simple identity layer 
	on top of the OAuth 2.0 protocol, which allows computing clients 
	to verify the identity of an end-user based on the authentication 
	performed by an authorization server, as well as to obtain 
	basic profile information about the end-user in an interoperable 
	and REST-like manner. In technical terms, OpenID Connect 
	specifies a RESTful HTTP API, using JSON as a data format.
	------------------------------------------------------------------------------
	(~) provides configuration methods of OpenID Connect
*/

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    internal class OpenIdConnectOptionsConfiguration : IConfigureNamedOptions<OpenIdConnectOptions>
    {
        private readonly IOptionsMonitor<AzureADOptions> _azureADOptions;

        public OpenIdConnectOptionsConfiguration(IOptionsMonitor<AzureADOptions> azureADOptions)
        {
            _azureADOptions = azureADOptions;
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
            // 'URI' - Uniform Resource Identifier
            options.Authority = new Uri(new Uri(azureADOptions.Instance), azureADOptions.TenantId).ToString();
            options.CallbackPath = azureADOptions.CallbackPath ?? options.CallbackPath;
            options.SignedOutCallbackPath = azureADOptions.SignedOutCallbackPath ?? options.SignedOutCallbackPath;
            options.SignInScheme = "Identity.External";
            options.UseTokenLifetime = true;
        }
        public void Configure(OpenIdConnectOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
