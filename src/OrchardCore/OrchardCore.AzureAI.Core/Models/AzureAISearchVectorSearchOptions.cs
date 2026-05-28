namespace OrchardCore.AzureAI.Models;

public sealed class AzureAISearchVectorSearchOptions
{
    public const string DefaultProfileName = "default";
    public const string DefaultAlgorithmName = "default-algorithm";

    public IList<AzureAISearchVectorSearchProfile> Profiles { get; init; } = [];

    public IList<AzureAISearchVectorSearchAlgorithm> Algorithms { get; init; } = [];
}
