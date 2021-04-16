using OrchardCore.Filters.Abstractions.Nodes;
using OrchardCore.Filters.Abstractions.Services;
using Parlot.Fluent;

namespace OrchardCore.Filters.Abstractions.Builders
{
    public abstract class OperatorEngineBuilder<T, TTermOption> where TTermOption : TermOption
    {
        public abstract (Parser<OperatorNode> Parser, TTermOption TermOption) Build();
    }
}
