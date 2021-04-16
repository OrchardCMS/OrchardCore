using OrchardCore.Filters.Abstractions.Services;

namespace OrchardCore.Filters.Abstractions.Nodes
{
    public abstract class FilterNode
    {
        public abstract string ToNormalizedString();

        public abstract TResult Accept<TArgument, TResult>(IFilterVisitor<TArgument, TResult> visitor, TArgument argument);
    }
}
