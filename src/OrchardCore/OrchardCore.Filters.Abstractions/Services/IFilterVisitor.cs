using OrchardCore.Filters.Abstractions.Nodes;

namespace OrchardCore.Filters.Abstractions.Services
{
    public interface IFilterVisitor<TArgument, TResult>
    {
        TResult Visit(TermNode node, TArgument argument);
        TResult Visit(TermOperationNode node, TArgument argument);
        TResult Visit(AndTermNode node, TArgument argument);
        TResult Visit(UnaryNode node, TArgument argument);
        TResult Visit(NotUnaryNode node, TArgument argument);
        TResult Visit(OrNode node, TArgument argument);
        TResult Visit(AndNode node, TArgument argument);
        TResult Visit(GroupNode node, TArgument argument);
    }
}
