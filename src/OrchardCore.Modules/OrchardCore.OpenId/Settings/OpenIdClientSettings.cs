namespace OrchardCore.OpenId.Settings;

public class OpenIdClientSettings
{
    public string DisplayName { get; set; }
    public Uri Authority { get; set; }
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret, protected with Data Protection.
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret of the Secrets module containing the client secret.
    /// When set, this takes precedence over <see cref="ClientSecret"/>.
    /// </summary>
    public string ClientSecretSecretName { get; set; }

    public string CallbackPath { get; set; }
    public string SignedOutRedirectUri { get; set; }
    public string SignedOutCallbackPath { get; set; }
    public IEnumerable<string> Scopes { get; set; }
    public string ResponseType { get; set; }
    public string ResponseMode { get; set; }
    public bool StoreExternalTokens { get; set; }
    public ParameterSetting[] Parameters { get; set; } = [];
}

public class ParameterSetting
{
    public string Name { get; set; }
    public string Value { get; set; }
}
