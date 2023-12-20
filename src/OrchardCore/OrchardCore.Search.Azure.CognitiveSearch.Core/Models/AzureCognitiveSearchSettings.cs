using OrchardCore.Contents.Indexing;

namespace OrchardCore.Search.Azure.CognitiveSearch.Models;

public class AzureCognitiveSearchSettings
{
    public static readonly string[] FullTextField = [CognitiveIndexingConstants.FullTextKey];

    public string[] DefaultSearchFields { get; set; } = FullTextField;

    public string SearchIndex { get; set; }
}
