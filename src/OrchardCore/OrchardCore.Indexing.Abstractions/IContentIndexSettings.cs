namespace OrchardCore.Indexing;

/// <summary>
/// Represents the indexing settings for a content part or a field.
/// </summary>
public interface IContentIndexSettings
{
    bool Included { get; set; }

    DocumentIndexOptions ToOptions();
}
