namespace OrchardCore.Indexing.Core.Models;

public class ContentIndexMetadata
{
    public bool IndexLatest { get; set; }

    public string[] IndexedContentTypes { get; set; }

    public string Culture { get; set; }
}
