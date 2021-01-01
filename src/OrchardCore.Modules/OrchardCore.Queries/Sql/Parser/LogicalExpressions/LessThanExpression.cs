namespace OrchardCore.Queries.Sql.Parser.LogicalExpressions
{
    public class LessThanExpression : BinaryExpression
    {
        public LessThanExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsNumber() < Right.EvaluateAsNumber();
    }
}
