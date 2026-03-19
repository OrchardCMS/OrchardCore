using Fluid;
using Fluid.Ast;

namespace OrchardCore.Queries.Sql;

internal static class SqlLiquidOutputExpressionDetector
{
    private static readonly FluidParser _parser = new();

    public static bool ContainsOutputStatement(string query)
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
