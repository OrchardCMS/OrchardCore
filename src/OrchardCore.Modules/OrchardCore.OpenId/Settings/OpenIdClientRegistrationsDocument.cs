using OrchardCore.Data.Documents;

namespace OrchardCore.OpenId.Settings;

/// <summary>
/// Stores the additional OpenID Connect client registrations of a tenant.
/// </summary>
public sealed class OpenIdClientRegistrationsDocument : Document
{
    /// <summary>
    /// Gets the client registrations, keyed by their name.
    /// </summary>
    public Dictionary<string, OpenIdClientRegistration> Registrations { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
