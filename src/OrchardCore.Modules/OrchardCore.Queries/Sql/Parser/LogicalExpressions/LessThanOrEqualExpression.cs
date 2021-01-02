namespace OrchardCore.Queries.Sql.Parser.Expressions
{
    public class LessThanOrEqualExpression : BinaryExpression
    {
        public LessThanOrEqualExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsBoolean() == Right.EvaluateAsBoolean();
    }
}
