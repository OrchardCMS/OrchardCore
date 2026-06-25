using YesSql.Filters.Nodes;

namespace OrchardCore.Contents.Services;

/// <summary>
/// Provides a filter node used when multiple content type stereotypes are specified.
/// </summary>
public class StereotypesFilterNode : TermOperationNode
{
    public StereotypesFilterNode(string[] stereotypes)
        : base("stereotypes", new UnaryNode(string.Join(',', stereotypes), OperateNodeQuotes.None))
    {
    }

    public override string ToNormalizedString()
        => string.Empty;

    public override string ToString()
        => string.Empty;
}
