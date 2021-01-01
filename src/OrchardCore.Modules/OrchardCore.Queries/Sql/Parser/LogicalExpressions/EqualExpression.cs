namespace OrchardCore.Queries.Sql.Parser.LogicalExpressions
{
    public class EqualExpression : BinaryExpression
    {
        public EqualExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsBoolean() == Right.EvaluateAsBoolean();
    }
}
