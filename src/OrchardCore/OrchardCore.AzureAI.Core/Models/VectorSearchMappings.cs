namespace OrchardCore.AzureAI.Models;

public sealed class VectorSearchMappings
{
    public const string DefaultProfileName = "default";
    public const string DefaultAlgorithmName = "default-algorithm";

    public IList<VectorSearchProfileMap> Profiles { get; init; } = [];

    public IList<VectorSearchAlgorithmMap> Algorithms { get; init; } = [];
}
