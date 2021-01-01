namespace OrchardCore.Queries.Sql.Parser.LogicalExpressions
{
    public class NotExpression : UnaryExpression
    {
        public NotExpression(Expression expression) : base(expression)
        {
        }

        public override object Evaluate() => !InnerExpression.EvaluateAsBoolean();
    }
}
