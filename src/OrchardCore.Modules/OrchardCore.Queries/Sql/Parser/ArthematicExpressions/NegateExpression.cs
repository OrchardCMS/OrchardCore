namespace OrchardCore.Queries.Sql.Parser.ArthematicExpressions
{
    public class NegateExpression : UnaryExpression
    {
        public NegateExpression(Expression expression) : base(expression)
        {
        }

        public override object Evaluate() => -1 * InnerExpression.EvaluateAsNumber();
    }
}
