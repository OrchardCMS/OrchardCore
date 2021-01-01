namespace OrchardCore.Queries.Sql.Parser.LogicalExpressions
{
    public class BitwiseAndExpressionn : BinaryExpression
    {
        public BitwiseAndExpressionn(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsInteger() & Right.EvaluateAsInteger();
    }
}
