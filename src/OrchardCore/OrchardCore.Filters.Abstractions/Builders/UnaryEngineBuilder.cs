using OrchardCore.Filters.Abstractions.Nodes;
using OrchardCore.Filters.Abstractions.Services;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace OrchardCore.Filters.Abstractions.Builders
{
    public abstract class UnaryEngineBuilder<T, TTermOption> : OperatorEngineBuilder<T, TTermOption> where TTermOption : TermOption
    {
        protected TTermOption _termOption;
        private static Parser<OperatorNode> _parser
            => Terms.String()
                .Or(
                    Terms.NonWhiteSpace()
                )
                    .Then<OperatorNode>((node) => new UnaryNode(node.ToString()));

        public UnaryEngineBuilder(TTermOption termOption)
        {
            _termOption = termOption;
        }

        public UnaryEngineBuilder<T, TTermOption> AllowMultiple()
        {
            _termOption.Single = false;

            return this;
        }

        public override (Parser<OperatorNode> Parser, TTermOption TermOption) Build()
            => (_parser, _termOption);


    }
}
