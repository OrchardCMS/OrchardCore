namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchDefaultSettings
{
    public bool UseCustomConfiguration { get; set; }

    public AzureAIAuthenticationType AuthenticationType { get; set; }

    public string Endpoint { get; set; }

    public string ApiKey { get; set; }

    public string IdentityClientId { get; set; }
}
