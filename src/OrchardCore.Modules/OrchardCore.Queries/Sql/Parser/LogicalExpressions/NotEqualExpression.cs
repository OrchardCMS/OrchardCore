namespace OrchardCore.Queries.Sql.Parser.Expressions
{
    public class NotEqualExpression : BinaryExpression
    {
        public NotEqualExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsBoolean() != Right.EvaluateAsBoolean();
    }
}
