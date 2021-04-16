using OrchardCore.Filters.Abstractions.Nodes;
using OrchardCore.Filters.Abstractions.Services;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace OrchardCore.Filters.Abstractions.Builders
{
    public class DefaultTermEngineBuilder<T, TTermOption> : TermEngineBuilder<T, TTermOption> where T : class where TTermOption : TermOption
    {
        public DefaultTermEngineBuilder(string name) : base(name)
        {
        }

        public override (Parser<TermNode> Parser, TTermOption TermOption) Build()
        {
            var op = _operatorParser.Build();

            var termParser = Terms.Text(Name, caseInsensitive: true)
                .AndSkip(Literals.Char(':'))
                .And(op.Parser)
                    .Then<TermNode>(x => new NamedTermNode(x.Item1, x.Item2));

            var defaultParser = op.Parser.Then<TermNode>(x => new DefaultTermNode(Name, x));

            var parser = termParser.Or(defaultParser);

            return (parser, op.TermOption);
        }
    }
}
