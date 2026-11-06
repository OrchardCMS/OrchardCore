namespace OrchardCore.OpenId.Settings;

/// <summary>
/// Represents an additional OpenID Connect client registration, used to connect to another identity provider
/// than the one configured in the <see cref="OpenIdClientSettings"/> of the site.
/// </summary>
public sealed class OpenIdClientRegistration : OpenIdClientSettings
{
    /// <summary>
    /// Gets or sets the unique name of the registration. It's used as the name of the authentication scheme and
    /// as the login provider of the user accounts linked to the identity provider, so it shouldn't be changed
    /// once users signed in with it.
    /// </summary>
    public string Name { get; set; }
}
