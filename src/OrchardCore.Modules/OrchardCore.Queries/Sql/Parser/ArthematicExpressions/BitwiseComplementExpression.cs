namespace OrchardCore.Queries.Sql.Parser.LogicalExpressions
{
    public class BitwiseComplementExpression : UnaryExpression
    {
        public BitwiseComplementExpression(Expression expression) : base(expression)
        {
        }

        public override object Evaluate() => ~InnerExpression.EvaluateAsInteger();
    }
}
