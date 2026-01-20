using YesSql.Filters.Abstractions.Nodes;

namespace OrchardCore.Contents.Services;

/// <summary>
/// Provides a content type node is used when a filter has not been selected.
/// </summary>
public class ContentTypeFilterNode : TermOperationNode
{
    public ContentTypeFilterNode(string selectedContentType) : base("type", new UnaryNode(selectedContentType, OperateNodeQuotes.None))
    {
    }

    public override string ToNormalizedString()
        => string.Empty;

    public override string ToString()
        => string.Empty;
}
