using YesSql.Filters.Nodes;

namespace OrchardCore.Contents.Services;

/// <summary>
/// Provides a content type node is used when a filter has not been selected.
/// </summary>
public class ContentTypeFilterNode : TermOperationNode
{
    public ContentTypeFilterNode(string selectedContentType)
        : base("type", new UnaryNode(selectedContentType, OperateNodeQuotes.None))
    {
    }

    /// <summary>
    /// Initializes a filter node for the specified content types.
    /// </summary>
    /// <param name="selectedContentTypes">The content type names to filter.</param>
    public ContentTypeFilterNode(string[] selectedContentTypes)
        : this(string.Join(',', selectedContentTypes))
    {
    }

    public override string ToNormalizedString()
        => string.Empty;

    public override string ToString()
        => string.Empty;
}
