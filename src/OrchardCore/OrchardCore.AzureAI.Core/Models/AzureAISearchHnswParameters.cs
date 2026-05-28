namespace OrchardCore.AzureAI.Models;

public sealed class AzureAISearchHnswParameters
{
    public int? M { get; set; }

    public int? EfConstruction { get; set; }

    public int? EfSearch { get; set; }

    public string Metric { get; set; }
}
