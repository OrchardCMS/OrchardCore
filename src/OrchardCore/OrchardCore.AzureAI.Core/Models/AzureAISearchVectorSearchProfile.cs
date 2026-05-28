namespace OrchardCore.AzureAI.Models;

public sealed class AzureAISearchVectorSearchProfile
{
    public string Name { get; set; }

    public string AlgorithmConfigurationName { get; set; }

    public string VectorizerName { get; set; }

    public string CompressionName { get; set; }
}
