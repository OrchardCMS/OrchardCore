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
using OpenIddict.Client.DataProtection;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Handlers;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Secrets;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.OpenId.Configuration;

public sealed class OpenIdClientConfiguration :
    IConfigureOptions<AuthenticationOptions>,
    IConfigureOptions<OpenIddictClientOptions>,
    IConfigureOptions<OpenIddictClientDataProtectionOptions>,
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
        ArgumentNullException.ThrowIfNull(options);

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
        ArgumentNullException.ThrowIfNull(options);

        var settings = GetClientSettingsAsync().GetAwaiter().GetResult();
        if (settings == null)
        {
            return;
        }

        // Note: the flows that are allowed to be negotiated by OpenIddict are currently not configurable in Orchard:
        // instead, the code, hybrid and implicit flows are assumed to be always supported at the global level.
        options.CodeChallengeMethods.Add(CodeChallengeMethods.Plain);
        options.CodeChallengeMethods.Add(CodeChallengeMethods.Sha256);

        options.GrantTypes.Add(GrantTypes.AuthorizationCode);
        options.GrantTypes.Add(GrantTypes.Implicit);

        options.ResponseModes.Add(ResponseModes.FormPost);
        options.ResponseModes.Add(ResponseModes.Fragment);
        options.ResponseModes.Add(ResponseModes.Query);

        options.ResponseTypes.Add(ResponseTypes.Code);
        options.ResponseTypes.Add(ResponseTypes.Code + ' ' + ResponseTypes.IdToken);
        options.ResponseTypes.Add(ResponseTypes.Code + ' ' + ResponseTypes.IdToken + ' ' + ResponseTypes.Token);
        options.ResponseTypes.Add(ResponseTypes.Code + ' ' + ResponseTypes.Token);
        options.ResponseTypes.Add(ResponseTypes.IdToken);
        options.ResponseTypes.Add(ResponseTypes.IdToken + ' ' + ResponseTypes.Token);
        options.ResponseTypes.Add(ResponseTypes.Token);

        // Note: the ASP.NET Core Data Protection integration is always used to generate
        // and validate tokens produced by the OpenIddict client. To ensure no exception
        // is thrown when no IdentityModel signing/encryption credentials were registered,
        // random symmetric signing and encryption keys are generated and registered here.
        options.EncryptionCredentials.Add(new EncryptingCredentials(new SymmetricSecurityKey(
            RandomNumberGenerator.GetBytes(256 / 8)), SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512));

        options.SigningCredentials.Add(new SigningCredentials(new SymmetricSecurityKey(
            RandomNumberGenerator.GetBytes(256 / 8)), SecurityAlgorithms.HmacSha256));

        options.Handlers.Add(OpenIdChallengeEventHandler.Descriptor);

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
                [nameof(OpenIdClientSettings)] = settings,
            },
        };

        if (!string.IsNullOrEmpty(settings.ResponseMode))
        {
            registration.ResponseModes.Add(settings.ResponseMode);
        }

        if (!string.IsNullOrEmpty(settings.ResponseType))
        {
            registration.ResponseTypes.Add(settings.ResponseType);
        }

        if (settings.Scopes is not null)
        {
            registration.Scopes.UnionWith(settings.Scopes);
        }

        registration.ClientSecret = _serviceProvider.GetSecretValueAsync(
            settings.ClientSecretSecretName,
            settings.ClientSecret,
            _dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration)),
            _logger)
            .GetAwaiter()
            .GetResult();

        options.Registrations.Add(registration);
    }

    public void Configure(OpenIddictClientDataProtectionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Note: currently, only the ASP.NET Core Data Protection format
        // is allowed for all the tokens produced by the OpenIddict client.
        options.PreferDefaultStateTokenFormat = false;
    }

    public void Configure(string name, OpenIddictClientAspNetCoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

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

    public void Configure(OpenIddictClientAspNetCoreOptions options)
        => Debug.Fail("This infrastructure method shouldn't be called.");

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
