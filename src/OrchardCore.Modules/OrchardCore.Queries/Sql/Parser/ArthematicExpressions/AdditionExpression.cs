namespace OrchardCore.Queries.Sql.Parser
{
    public class AdditionExpression : BinaryExpression
    {
        public AdditionExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsDecimal() + Right.EvaluateAsDecimal();
    }
}
