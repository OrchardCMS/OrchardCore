using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Services;

/// <summary>
/// Manages the additional OpenID Connect client registrations of a tenant, used to connect to several identity
/// providers. The registration configured in the <see cref="OpenIdClientSettings"/> of the site is managed by
/// <see cref="IOpenIdClientService"/>.
/// </summary>
public interface IOpenIdClientRegistrationService
{
    /// <summary>
    /// Gets the client registrations from the cache, for read-only use.
    /// </summary>
    Task<IReadOnlyCollection<OpenIdClientRegistration>> GetRegistrationsAsync();

    /// <summary>
    /// Gets a client registration from the cache, for read-only use.
    /// </summary>
    /// <param name="name">The name of the registration.</param>
    /// <returns>The registration, or <see langword="null"/> when no registration has this name.</returns>
    Task<OpenIdClientRegistration> GetRegistrationAsync(string name);

    /// <summary>
    /// Loads a client registration from the store, to be updated.
    /// </summary>
    /// <param name="name">The name of the registration.</param>
    /// <returns>The registration, or <see langword="null"/> when no registration has this name.</returns>
    Task<OpenIdClientRegistration> LoadRegistrationAsync(string name);

    /// <summary>
    /// Adds a client registration, or replaces the registration that has the same name.
    /// </summary>
    /// <param name="registration">The registration to save.</param>
    Task UpdateRegistrationAsync(OpenIdClientRegistration registration);

    /// <summary>
    /// Removes a client registration.
    /// </summary>
    /// <param name="name">The name of the registration.</param>
    /// <returns><see langword="true"/> when the registration was removed, <see langword="false"/> when it didn't exist.</returns>
    Task<bool> DeleteRegistrationAsync(string name);

    /// <summary>
    /// Validates a client registration before it's saved.
    /// </summary>
    /// <param name="registration">The registration to validate.</param>
    /// <param name="originalName">The name of the registration being edited, or <see langword="null"/> for a new registration.</param>
    Task<ImmutableArray<ValidationResult>> ValidateRegistrationAsync(OpenIdClientRegistration registration, string originalName = null);
}
