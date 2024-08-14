using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Configuration;

public sealed class OpenIdClientConfiguration :
    IConfigureOptions<AuthenticationOptions>,
    IConfigureOptions<OpenIddictClientOptions>,
    IConfigureNamedOptions<OpenIddictClientAspNetCoreOptions>
{
    private readonly IOpenIdClientService _clientService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public OpenIdClientConfiguration(
        IOpenIdClientService clientService,
        IDataProtectionProvider dataProtectionProvider,
        IServiceProvider serviceProvider,
        ShellSettings shellSettings,
        ILogger<OpenIdClientConfiguration> logger)
    {
        _clientService = clientService;
        _dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(AuthenticationOptions options)
    {
        var settings = GetClientSettingsAsync().GetAwaiter().GetResult();
        if (settings == null)
        {
            return;
        }

        options.AddScheme<OpenIddictClientAspNetCoreHandler>(
            OpenIddictClientAspNetCoreDefaults.AuthenticationScheme, displayName: null);

        foreach (var scheme in _serviceProvider.GetRequiredService<IOptionsMonitor<OpenIddictClientAspNetCoreOptions>>()
            .CurrentValue.ForwardedAuthenticationSchemes)
        {
            options.AddScheme<OpenIddictClientAspNetCoreForwarder>(scheme.Name, scheme.DisplayName);
        }
    }

    public void Configure(OpenIddictClientOptions options)
    {
        var settings = GetClientSettingsAsync().GetAwaiter().GetResult();
        if (settings == null)
        {
            return;
        }

        // Note: the provider name, redirect URI and post-logout redirect URI use the same default
        // values as the Microsoft ASP.NET Core OpenID Connect handler, for compatibility reasons.
        var registration = new OpenIddictClientRegistration
        {
            Issuer = settings.Authority,
            ClientId = settings.ClientId,
            RedirectUri = new Uri(settings.CallbackPath ?? "signin-oidc", UriKind.RelativeOrAbsolute),
            PostLogoutRedirectUri = new Uri(settings.SignedOutCallbackPath ?? "signout-callback-oidc", UriKind.RelativeOrAbsolute),
            ProviderName = "OpenIdConnect",
            ProviderDisplayName = settings.DisplayName,
            Properties =
            {
                [nameof(OpenIdClientSettings)] = settings
            }
        };

        if (!string.IsNullOrEmpty(settings.ResponseMode))
        {
            registration.ResponseModes.Add(settings.ResponseMode);
        }

        if (!string.IsNullOrEmpty(settings.ResponseType))
        {
            registration.ResponseTypes.Add(settings.ResponseType);
        }

        if (settings.Scopes != null)
        {
            registration.Scopes.UnionWith(settings.Scopes);
        }

        if (!string.IsNullOrEmpty(settings.ClientSecret))
        {
            var protector = _dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration));

            try
            {
                registration.ClientSecret = protector.Unprotect(settings.ClientSecret);
            }
            catch
            {
                _logger.LogError("The client secret could not be decrypted. It may have been encrypted using a different key.");
            }
        }

        options.Registrations.Add(registration);

        // Note: claims are mapped by CallbackController, so the built-in mapping feature is unnecessary.
        options.DisableWebServicesFederationClaimMapping = true;

        // TODO: use proper encryption/signing credentials, similar to what's used for the server feature.
        options.EncryptionCredentials.Add(new EncryptingCredentials(new SymmetricSecurityKey(
            RandomNumberGenerator.GetBytes(256 / 8)), SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512));

        options.SigningCredentials.Add(new SigningCredentials(new SymmetricSecurityKey(
            RandomNumberGenerator.GetBytes(256 / 8)), SecurityAlgorithms.HmacSha256));
    }

    public void Configure(string name, OpenIddictClientAspNetCoreOptions options)
    {
        // Note: the OpenID module handles the redirection requests in its dedicated
        // ASP.NET Core MVC controller, which requires enabling the pass-through mode.
        options.EnableRedirectionEndpointPassthrough = true;
        options.EnablePostLogoutRedirectionEndpointPassthrough = true;

        // Note: error pass-through is enabled to allow the actions of the MVC callback controller
        // to handle the errors returned by the interactive endpoints without relying on the generic
        // status code pages middleware to rewrite the response later in the request processing.
        options.EnableErrorPassthrough = true;

        // Note: in Orchard, transport security is usually configured via the dedicated HTTPS module.
        // To make configuration easier and avoid having to configure it in two different features,
        // the transport security requirement enforced by OpenIddict by default is always turned off.
        options.DisableTransportSecurityRequirement = true;
    }

    public void Configure(OpenIddictClientAspNetCoreOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

    private async Task<OpenIdClientSettings> GetClientSettingsAsync()
    {
        var settings = await _clientService.GetSettingsAsync();

        var result = await _clientService.ValidateSettingsAsync(settings);

        if (result.Any(x => x != ValidationResult.Success))
        {
            if (_shellSettings.IsRunning())
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    var errors = result.Where(x => x != ValidationResult.Success)
                        .Select(x => x.ErrorMessage);

                    _logger.LogWarning("The OpenID client settings are invalid: {Errors}", string.Join(System.Environment.NewLine, errors));
                }

                return null;
            }
        }

        return settings;
    }
}
