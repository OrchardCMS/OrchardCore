using YesSql.Filters.Nodes;

namespace OrchardCore.Contents.Services;

public class StereotypeFilterNode : TermOperationNode
{
    public StereotypeFilterNode(string stereotype)
        : base("stereotype", new UnaryNode(stereotype, OperateNodeQuotes.None))
    {
    }

    /// <summary>
    /// Initializes a filter node for the specified stereotypes.
    /// </summary>
    /// <param name="stereotypes">The stereotypes to filter.</param>
    public StereotypeFilterNode(string[] stereotypes)
        : this(string.Join(',', stereotypes))
    {
    }

    public override string ToNormalizedString()
        => string.Empty;

    public override string ToString()
        => string.Empty;
}
