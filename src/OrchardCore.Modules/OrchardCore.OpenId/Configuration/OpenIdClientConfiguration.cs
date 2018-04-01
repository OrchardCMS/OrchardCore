using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.OpenId.Services;

namespace OrchardCore.OpenId.Configuration
{
    [Feature(OpenIdConstants.Features.Client)]
    public class OpenIdClientConfiguration : IConfigureOptions<AuthenticationOptions>, IConfigureNamedOptions<OpenIdConnectOptions>
    {
        private readonly IOpenIdClientService _openIdConnectService;
        private readonly ILogger<OpenIdClientConfiguration> _logger;

        public OpenIdClientConfiguration(IOpenIdClientService openIdConnectService, ILogger<OpenIdClientConfiguration> logger)
        {
            _openIdConnectService = openIdConnectService;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            var settings = _openIdConnectService.GetOpenIdConnectSettings().GetAwaiter().GetResult();
            if (!_openIdConnectService.IsValidOpenIdConnectSettings(settings))
            {
                _logger.LogWarning("The OpenID Connect module is not correctly configured.");
                return;
            }

            // Register the OpenIddict handler in the authentication handlers collection.
            options.AddScheme(OpenIdConnectDefaults.AuthenticationScheme, builder =>
            {
                builder.DisplayName = settings.DisplayName;
                builder.HandlerType = typeof(OpenIdConnectHandler);
            });
        }

        public void Configure(string name, OpenIdConnectOptions options)
        {
            var settings = _openIdConnectService.GetOpenIdConnectSettings().GetAwaiter().GetResult();
            if (name != OpenIdConnectDefaults.AuthenticationScheme)
                return;

            if (_openIdConnectService.IsValidOpenIdConnectSettings(settings))
            {
                options.Authority = settings.Authority;
                options.ClientId = settings.ClientId;
                options.SignedOutRedirectUri = settings.SignedOutRedirectUri;
                options.SignedOutCallbackPath = settings.SignedOutCallbackPath;

                options.RequireHttpsMetadata = !settings.TestingModeEnabled;

                options.ResponseType = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectResponseType.IdToken;
                options.CallbackPath = settings.CallbackPath;

                foreach (var item in settings?.AllowedScopes)
                {
                    options.Scope.Add(item);
                }

                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };

                if (!string.IsNullOrWhiteSpace(settings.ClientSecret))
                    options.ClientSecret = _openIdConnectService.Unprotect(settings.ClientSecret);
            }
        }

        public void Configure(OpenIdConnectOptions options) { }

    }
}
