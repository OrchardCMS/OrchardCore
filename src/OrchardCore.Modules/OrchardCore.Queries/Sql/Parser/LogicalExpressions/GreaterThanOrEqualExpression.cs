namespace OrchardCore.Queries.Sql.Parser.Expressions
{
    public class GreaterThanOrEqualExpression : BinaryExpression
    {
        public GreaterThanOrEqualExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsDecimal() >= Right.EvaluateAsDecimal();
    }
}
