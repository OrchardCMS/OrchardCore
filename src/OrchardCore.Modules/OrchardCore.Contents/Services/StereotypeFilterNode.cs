using System;
using YesSql.Filters.Abstractions.Nodes;

namespace OrchardCore.Contents.Services;

public class StereotypeFilterNode : TermOperationNode
{
    public StereotypeFilterNode(string stereotype)
        : base("stereotype", new UnaryNode(stereotype, OperateNodeQuotes.None))
    {
    }

    public override string ToNormalizedString()
        => String.Empty;

    public override string ToString()
        => String.Empty;
}
