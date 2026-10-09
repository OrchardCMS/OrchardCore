namespace OrchardCore.AzureAI.Models;

public class AzureAISearchDefaultSettings
{
    public bool UseCustomConfiguration { get; set; }

    public AzureAIAuthenticationType AuthenticationType { get; set; }

    public string Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the API key, protected with Data Protection.
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret of the Secrets module containing the API key.
    /// When set, this takes precedence over <see cref="ApiKey"/>.
    /// </summary>
    public string ApiKeySecretName { get; set; }

    public string IdentityClientId { get; set; }
}
