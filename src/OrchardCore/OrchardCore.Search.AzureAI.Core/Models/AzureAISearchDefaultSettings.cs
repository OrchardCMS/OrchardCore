using System;

namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchDefaultSettings
{
    public bool UseCustomConfiguration { get; set; }

    public AzureAIAuthenticationType AuthenticationType { get; set; }

    public string Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the API key.
    /// </summary>
    [Obsolete("This property is deprecated and will be removed in a future version. Use the Secrets module to store sensitive data and reference it via ApiKeySecretName.")]
    public string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret containing the API key.
    /// When set, this takes precedence over the ApiKey property.
    /// </summary>
    public string ApiKeySecretName { get; set; }

    public string IdentityClientId { get; set; }
}
