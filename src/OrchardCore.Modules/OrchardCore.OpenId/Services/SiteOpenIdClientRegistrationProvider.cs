using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using OpenIddict.Client;
using OrchardCore.Environment.Shell;

namespace OrchardCore.OpenId.Services;

/// <summary>
/// Provides the client registration configured in the OpenID Connect client settings of the site.
/// </summary>
public sealed class SiteOpenIdClientRegistrationProvider : IOpenIdClientRegistrationProvider
{
    private readonly IOpenIdClientService _clientService;
    private readonly OpenIdClientRegistrationFactory _registrationFactory;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public SiteOpenIdClientRegistrationProvider(
        IOpenIdClientService clientService,
        OpenIdClientRegistrationFactory registrationFactory,
        ShellSettings shellSettings,
        ILogger<SiteOpenIdClientRegistrationProvider> logger)
    {
        _clientService = clientService;
        _registrationFactory = registrationFactory;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public async Task<IEnumerable<OpenIddictClientRegistration>> GetRegistrationsAsync()
    {
        var settings = await _clientService.GetSettingsAsync();

        // The settings are empty until the client is configured, which isn't worth a warning.
        if (settings.Authority is null && string.IsNullOrEmpty(settings.ClientId))
        {
            return [];
        }

        var errors = (await _clientService.ValidateSettingsAsync(settings))
            .Where(result => result != ValidationResult.Success)
            .Select(result => result.ErrorMessage)
            .ToArray();

        if (errors.Length > 0)
        {
            if (_shellSettings.IsRunning() && _logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("The OpenID client settings are invalid: {Errors}", string.Join(System.Environment.NewLine, errors));
            }

            return [];
        }

        // Note: the provider name, redirect URI and post-logout redirect URI use the same default values
        // as the Microsoft ASP.NET Core OpenID Connect handler, so the user logins and the URIs registered
        // in the identity provider keep working.
        return
        [
            await _registrationFactory.CreateAsync(
                settings,
                OpenIdConstants.Client.DefaultProviderName,
                OpenIdConstants.Client.DefaultCallbackPath,
                OpenIdConstants.Client.DefaultSignedOutCallbackPath),
        ];
    }
}
