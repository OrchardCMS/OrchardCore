namespace OrchardCore.Indexing.Core.Models;

public class ContentIndexEntityMetadata
{
    public bool IndexLatest { get; set; }

    public string[] IndexedContentTypes { get; set; }

    public string Culture { get; set; }
}
