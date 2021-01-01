namespace OrchardCore.Queries.Sql.Parser.LogicalExpressions
{
    public class AndExpression : BinaryExpression
    {
        public AndExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsBoolean() && Right.EvaluateAsBoolean();
    }
}
