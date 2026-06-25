using YesSql.Filters.Nodes;

namespace OrchardCore.Contents.Services;

/// <summary>
/// Provides a filter node used when multiple content type IDs are specified.
/// </summary>
public class ContentTypesFilterNode : TermOperationNode
{
    public ContentTypesFilterNode(string[] contentTypeIds)
        : base("types", new UnaryNode(string.Join(',', contentTypeIds), OperateNodeQuotes.None))
    {
    }

    public override string ToNormalizedString()
        => string.Empty;

    public override string ToString()
        => string.Empty;
}
