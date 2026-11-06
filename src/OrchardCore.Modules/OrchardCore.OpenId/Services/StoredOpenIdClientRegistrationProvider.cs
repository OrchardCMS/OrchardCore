using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using OpenIddict.Client;

namespace OrchardCore.OpenId.Services;

/// <summary>
/// Provides the additional client registrations managed in the admin, stored in the
/// <see cref="Settings.OpenIdClientRegistrationsDocument"/> of the tenant.
/// </summary>
public sealed class StoredOpenIdClientRegistrationProvider : IOpenIdClientRegistrationProvider
{
    private readonly IOpenIdClientRegistrationService _registrationService;
    private readonly IOpenIdClientService _clientService;
    private readonly OpenIdClientRegistrationFactory _registrationFactory;
    private readonly ILogger _logger;

    public StoredOpenIdClientRegistrationProvider(
        IOpenIdClientRegistrationService registrationService,
        IOpenIdClientService clientService,
        OpenIdClientRegistrationFactory registrationFactory,
        ILogger<StoredOpenIdClientRegistrationProvider> logger)
    {
        _registrationService = registrationService;
        _clientService = clientService;
        _registrationFactory = registrationFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<OpenIddictClientRegistration>> GetRegistrationsAsync()
    {
        var registrations = new List<OpenIddictClientRegistration>();

        foreach (var settings in await _registrationService.GetRegistrationsAsync())
        {
            // The registrations are validated when they are saved, but a registration imported
            // by a recipe could still be invalid and would prevent the client from starting.
            if ((await _clientService.ValidateSettingsAsync(settings)).Any(result => result != ValidationResult.Success))
            {
                _logger.LogWarning("The OpenID client registration '{Name}' is invalid and is ignored.", settings.Name);

                continue;
            }

            var name = settings.Name.ToLowerInvariant();

            registrations.Add(await _registrationFactory.CreateAsync(
                settings,
                settings.Name,
                $"{OpenIdConstants.Client.DefaultCallbackPath}-{name}",
                $"{OpenIdConstants.Client.DefaultSignedOutCallbackPath}-{name}"));
        }

        return registrations;
    }
}
