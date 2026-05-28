namespace OrchardCore.AzureAI.Models;

public sealed class AzureAISearchVectorSearchAlgorithm
{
    public const string HnswKind = "hnsw";
    public const string ExhaustiveKnnKind = "exhaustiveKnn";

    public string Name { get; set; }

    public string Kind { get; set; } = HnswKind;

    public AzureAISearchHnswParameters HnswParameters { get; set; }

    public AzureAISearchExhaustiveKnnParameters ExhaustiveKnnParameters { get; set; }
}
