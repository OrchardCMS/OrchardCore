using OpenIddict.Client;

namespace OrchardCore.OpenId.Services;

/// <summary>
/// Provides OpenIddict client registrations to the OpenID Connect client feature, which registers an
/// authentication scheme and the callback endpoints of each one.
/// </summary>
/// <remarks>
/// <para>
/// Implementations are registered as singletons with <c>services.AddOpenIdClientRegistrationProvider&lt;T&gt;()</c>,
/// and are invoked once, when the tenant builds its OpenIddict client options. A change to the data a provider
/// reads only applies once the tenant is released, e.g. with <c>IShellReleaseManager.RequestRelease()</c>.
/// </para>
/// <para>
/// Each registration needs a unique <see cref="OpenIddictClientRegistration.ProviderName"/>, used as its
/// authentication scheme, a <see cref="OpenIddictClientRegistration.ProviderDisplayName"/> shown on the login
/// page, and a relative <see cref="OpenIddictClientRegistration.RedirectUri"/>, without a leading <c>/</c> so it's
/// resolved against the base path of the tenant. To use one of the OpenIddict web providers, set
/// <see cref="OpenIddictClientRegistration.ProviderType"/> and <see cref="OpenIddictClientRegistration.ProviderSettings"/>
/// instead of <see cref="OpenIddictClientRegistration.Issuer"/>.
/// </para>
/// </remarks>
public interface IOpenIdClientRegistrationProvider
{
    /// <summary>
    /// Gets the client registrations to add to the OpenIddict client options of the tenant.
    /// </summary>
    Task<IEnumerable<OpenIddictClientRegistration>> GetRegistrationsAsync();
}
