namespace OrchardCore.Search.Elasticsearch.Models;

public class ContentIndexMetadata
{
    public bool IndexLatest { get; set; }

    public string[] IndexedContentTypes { get; set; }

    public string Culture { get; set; }

    public bool StoreSourceData { get; set; }
}
