namespace OrchardCore.Queries.Sql.Parser.Expressions
{
    public class BitwiseComplementExpression : UnaryExpression
    {
        public BitwiseComplementExpression(Expression expression) : base(expression)
        {
        }

        public override object Evaluate() => ~InnerExpression.EvaluateAsInteger();
    }
}
