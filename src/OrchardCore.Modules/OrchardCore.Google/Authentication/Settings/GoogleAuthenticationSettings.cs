using Microsoft.AspNetCore.Http;

namespace OrchardCore.Google.Authentication.Settings;

public class GoogleAuthenticationSettings
{
    public string ClientID { get; set; }

    /// <summary>
    /// Gets or sets the client secret, protected with Data Protection.
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret of the Secrets module containing the client secret.
    /// When set, this takes precedence over <see cref="ClientSecret"/>.
    /// </summary>
    public string ClientSecretSecretName { get; set; }

    public PathString CallbackPath { get; set; }
    public bool SaveTokens { get; set; }
}
