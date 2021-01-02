namespace OrchardCore.Queries.Sql.Parser.Expressions
{
    public class NegateExpression : UnaryExpression
    {
        public NegateExpression(Expression expression) : base(expression)
        {
        }

        public override object Evaluate() => -1 * InnerExpression.EvaluateAsDecimal();
    }
}
