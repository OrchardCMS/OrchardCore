namespace OrchardCore.Search.Elasticsearch.Core.Mappings;

internal sealed class DisplayTextModel
{
    public string Analyzed { get; set; }

    public string Normalized { get; set; }

    public string Keyword { get; set; }
}
