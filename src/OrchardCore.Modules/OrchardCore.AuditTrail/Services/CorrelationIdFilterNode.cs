using YesSql.Filters.Abstractions.Nodes;

namespace OrchardCore.AuditTrail.Services;

/// <summary>
/// Provides a correlation id node is used when a filter has not been selected.
/// </summary>
public class CorrelationIdFilterNode : TermOperationNode
{
    public CorrelationIdFilterNode(string correlationId) : base("id", new UnaryNode(correlationId, OperateNodeQuotes.None))
    {
    }

    public override string ToNormalizedString()
        => string.Empty;

    public override string ToString()
        => string.Empty;
}
