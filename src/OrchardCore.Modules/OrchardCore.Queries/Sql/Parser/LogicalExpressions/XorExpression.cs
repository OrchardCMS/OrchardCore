namespace OrchardCore.Queries.Sql.Parser.LogicalExpressions
{
    public class XorExpression : BinaryExpression
    {
        public XorExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsBoolean() ^ Right.EvaluateAsBoolean();
    }
}
