namespace OrchardCore.Queries.Sql.Parser.LogicalExpressions
{
    public class OrExpression : BinaryExpression
    {
        public OrExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsBoolean() || Right.EvaluateAsBoolean();
    }
}
