using Fluid;
using Fluid.Ast;
using OrchardCore.DisplayManagement.Liquid;

namespace OrchardCore.Queries.Sql;

public sealed class SqlLiquidOutputExpressionDetector
{
    private readonly FluidParser _parser;

    public SqlLiquidOutputExpressionDetector(LiquidViewParser parser)
        : this((FluidParser)parser)
    {
    }

    internal SqlLiquidOutputExpressionDetector(FluidParser parser)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    public bool ContainsOutputStatement(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return false;
        }

        if (!_parser.TryParse(query, out var template, out _))
        {
            return false;
        }

        var visitor = new OutputStatementDetector();
        visitor.VisitTemplate(template);

        return visitor.ContainsOutputStatement;
    }

    private sealed class OutputStatementDetector : AstVisitor
    {
        public bool ContainsOutputStatement { get; private set; }

        protected override Fluid.Ast.Statement VisitOutputStatement(OutputStatement statement)
        {
            ContainsOutputStatement = true;

            return statement;
        }
    }
}
