namespace OrchardCore.AzureAI.Models;

public sealed class VectorSearchAlgorithmMap
{
    public const string HnswKind = "hnsw";
    public const string ExhaustiveKnnKind = "exhaustiveKnn";

    public string Name { get; set; }

    public string Kind { get; set; } = HnswKind;

    public HnswParametersMap HnswParametersMap { get; set; }

    public ExhaustiveKnnParametersMap ExhaustiveKnnParametersMap { get; set; }
}
