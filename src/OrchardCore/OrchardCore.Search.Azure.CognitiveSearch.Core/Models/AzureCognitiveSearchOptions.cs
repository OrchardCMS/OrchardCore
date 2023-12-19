using Azure;

namespace OrchardCore.Search.Azure.CognitiveSearch.Models;

public class AzureCognitiveSearchOptions
{
    public string Endpoint { get; set; }

    public AzureKeyCredential Credential { get; set; }

    public string IndexPrefix { get; set; }
}
