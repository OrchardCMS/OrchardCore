using Microsoft.Extensions.Logging;
using OpenIddict.Client;

namespace OrchardCore.OpenId.Services;

/// <summary>
/// Collects the client registrations of all the <see cref="IOpenIdClientRegistrationProvider"/> once per tenant,
/// so the authentication options, the OpenIddict client options and the callback routes agree on them.
/// </summary>
public sealed class OpenIdClientRegistrationResolver
{
    private readonly IEnumerable<IOpenIdClientRegistrationProvider> _providers;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private IReadOnlyList<OpenIddictClientRegistration> _registrations;

    public OpenIdClientRegistrationResolver(
        IEnumerable<IOpenIdClientRegistrationProvider> providers,
        ILogger<OpenIdClientRegistrationResolver> logger)
    {
        _providers = providers;
        _logger = logger;
    }

    /// <summary>
    /// Gets the client registrations of the tenant, with a unique provider name.
    /// </summary>
    public async Task<IReadOnlyList<OpenIddictClientRegistration>> GetRegistrationsAsync()
    {
        if (_registrations is not null)
        {
            return _registrations;
        }

        await _semaphore.WaitAsync();

        try
        {
            return _registrations ??= await ResolveAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<IReadOnlyList<OpenIddictClientRegistration>> ResolveAsync()
    {
        var registrations = new List<OpenIddictClientRegistration>();
        var providerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var provider in _providers)
        {
            foreach (var registration in await provider.GetRegistrationsAsync())
            {
                // The provider name is the authentication scheme of the registration, so two registrations sharing
                // it would make the OpenIddict client fail to start. The first one wins, in feature order.
                if (!string.IsNullOrEmpty(registration.ProviderName) && !providerNames.Add(registration.ProviderName))
                {
                    _logger.LogWarning(
                        "The OpenID client registration '{ProviderName}' is ignored, as another registration uses the same provider name.",
                        registration.ProviderName);

                    continue;
                }

                registrations.Add(registration);
            }
        }

        return registrations;
    }
}
