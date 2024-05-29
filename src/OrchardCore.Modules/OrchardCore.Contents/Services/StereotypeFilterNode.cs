using YesSql.Filters.Abstractions.Nodes;

namespace OrchardCore.Contents.Services;

public class StereotypeFilterNode : TermOperationNode
{
    public StereotypeFilterNode(string stereotype)
        : base("stereotype", new UnaryNode(stereotype, OperateNodeQuotes.None))
    {
    }

    public override string ToNormalizedString()
        => string.Empty;

    public override string ToString()
        => string.Empty;
}
