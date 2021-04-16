using OrchardCore.Filters.Abstractions.Nodes;
using OrchardCore.Filters.Abstractions.Services;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace OrchardCore.Filters.Abstractions.Builders
{
    public class NamedTermEngineBuilder<T, TTermOption> : TermEngineBuilder<T, TTermOption> where TTermOption : TermOption
    {
        public NamedTermEngineBuilder(string name) : base(name)
        {
        }

        public override (Parser<TermNode> Parser, TTermOption TermOption) Build()
        {
            var op = _operatorParser.Build();

            var parser = Terms.Text(Name, caseInsensitive: true)
                .AndSkip(Literals.Char(':'))
                .And(op.Parser)
                    .Then<TermNode>(x => new NamedTermNode(x.Item1, x.Item2));

            return (parser, op.TermOption);
        }                    
    }
}
