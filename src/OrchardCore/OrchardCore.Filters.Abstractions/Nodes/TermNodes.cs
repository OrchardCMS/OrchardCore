using System.Collections.Generic;
using System.Linq;
using OrchardCore.Filters.Abstractions.Services;

namespace OrchardCore.Filters.Abstractions.Nodes
{
    public abstract class TermNode : FilterNode
    {
        public TermNode(string termName)
        {
            TermName = termName;
        }

        public string TermName { get; }
        public override TResult Accept<TArgument, TResult>(IFilterVisitor<TArgument, TResult> visitor, TArgument argument)
            => visitor.Visit(this, argument);
    }

    public abstract class TermOperationNode : TermNode
    {
        public TermOperationNode(string termName, OperatorNode operation) : base(termName)
        {
            Operation = operation;
        }

        public OperatorNode Operation { get; }

        public override TResult Accept<TArgument, TResult>(IFilterVisitor<TArgument, TResult> visitor, TArgument argument)
            => visitor.Visit(this, argument);
    }

    public class NamedTermNode : TermOperationNode
    {
        public NamedTermNode(string termName, OperatorNode operation) : base(termName, operation)
        {
        }

        public override string ToNormalizedString()
            => $"{TermName}:{Operation.ToNormalizedString()}";

        public override string ToString()
            => $"{TermName}:{Operation.ToString()}";
    }


    public class DefaultTermNode : TermOperationNode
    {
        public DefaultTermNode(string termName, OperatorNode operation) : base(termName, operation)
        {
        }

        public override string ToNormalizedString() // normalizing includes the term name even if not specified.
            => $"{TermName}:{Operation.ToNormalizedString()}";

        public override string ToString()
            => $"{Operation.ToString()}";
    }

    public abstract class CompoundTermNode : TermNode
    {
        public CompoundTermNode(string termName) : base(termName)
        {
        }

        public List<TermOperationNode> Children { get; } = new List<TermOperationNode>();
    }

    public class AndTermNode : CompoundTermNode
    {
        public AndTermNode(TermOperationNode existingTerm, TermOperationNode newTerm) : base(existingTerm.TermName)
        {
            Children.Add(existingTerm);
            Children.Add(newTerm);
        }

        public override TResult Accept<TArgument, TResult>(IFilterVisitor<TArgument, TResult> visitor, TArgument argument)
            => visitor.Visit(this, argument);

        public override string ToNormalizedString()
            => string.Join(" ", Children.Select(c => c.ToNormalizedString()));

        public override string ToString()
            => string.Join(" ", Children.Select(c => c.ToString()));
    }
}
