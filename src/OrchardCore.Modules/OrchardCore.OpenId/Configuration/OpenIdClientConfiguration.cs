using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Configuration
{
    [Feature(OpenIdConstants.Features.Client)]
    public class OpenIdClientConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<OpenIdConnectOptions>
    {
        private readonly IOpenIdClientService _clientService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ShellSettings _shellSettings;
        private readonly OpenIdClientSettings _openIdClientSettings;
        private readonly ILogger _logger;

        public OpenIdClientConfiguration(
            IOpenIdClientService clientService,
            IDataProtectionProvider dataProtectionProvider,
            ShellSettings shellSettings,
            IOptions<OpenIdClientSettings> openIdClientSettings,
            ILogger<OpenIdClientConfiguration> logger)
        {
            _clientService = clientService;
            _dataProtectionProvider = dataProtectionProvider;
            _shellSettings = shellSettings;
            _openIdClientSettings = openIdClientSettings.Value;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            if (_openIdClientSettings == null)
            {
                return;
            }

            // Register the OpenID Connect client handler in the authentication handlers collection.
            options.AddScheme<OpenIdConnectHandler>(OpenIdConnectDefaults.AuthenticationScheme, _openIdClientSettings.DisplayName);
        }

        public void Configure(string name, OpenIdConnectOptions options)
        {
            // Ignore OpenID Connect client handler instances that don't correspond to the instance managed by the OpenID module.
            if (!string.Equals(name, OpenIdConnectDefaults.AuthenticationScheme))
            {
                return;
            }

            if (_openIdClientSettings == null)
            {
                return;
            }

            options.Authority = _openIdClientSettings.Authority.AbsoluteUri;
            options.ClientId = _openIdClientSettings.ClientId;
            options.SignedOutRedirectUri = _openIdClientSettings.SignedOutRedirectUri ?? options.SignedOutRedirectUri;
            options.SignedOutCallbackPath = _openIdClientSettings.SignedOutCallbackPath ?? options.SignedOutCallbackPath;
            options.RequireHttpsMetadata = string.Equals(_openIdClientSettings.Authority.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
            options.GetClaimsFromUserInfoEndpoint = true;
            options.ResponseMode = _openIdClientSettings.ResponseMode;
            options.ResponseType = _openIdClientSettings.ResponseType;
            options.SaveTokens = _openIdClientSettings.StoreExternalTokens;

            options.CallbackPath = _openIdClientSettings.CallbackPath ?? options.CallbackPath;

            if (_openIdClientSettings.Scopes != null)
            {
                foreach (var scope in _openIdClientSettings.Scopes)
                {
                    options.Scope.Add(scope);
                }
            }

            if (!string.IsNullOrEmpty(_openIdClientSettings.ClientSecret))
            {
                var protector = _dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration));

                try
                {
                    options.ClientSecret = protector.Unprotect(_openIdClientSettings.ClientSecret);
                }
                catch
                {
                    _logger.LogError("The client secret could not be decrypted. It may have been encrypted using a different key.");
                }
            }

            if (_openIdClientSettings.Parameters != null && _openIdClientSettings.Parameters.Length > 0)
            {
                var parameters = _openIdClientSettings.Parameters;
                options.Events.OnRedirectToIdentityProvider = (context) =>
                {
                    foreach (var parameter in parameters)
                    {
                        context.ProtocolMessage.SetParameter(parameter.Name, parameter.Value);
                    }

                    return Task.CompletedTask;
                };
            }
        }

        public void Configure(OpenIdConnectOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
