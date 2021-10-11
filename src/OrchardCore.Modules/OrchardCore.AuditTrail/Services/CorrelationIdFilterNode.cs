using System;
using YesSql.Filters.Abstractions.Nodes;

namespace OrchardCore.AuditTrail.Services
{
    /// <summary>
    /// Provides a correlation id node is used when a filter has not been selected
    /// </summary>
    public class CorrelationIdFilterNode : TermOperationNode
    {
        public CorrelationIdFilterNode(string correlationId) : base("id", new UnaryNode(correlationId))
        {
        }

        public override string ToNormalizedString()
            => String.Empty;

        public override string ToString()
            => String.Empty;
    }
}
