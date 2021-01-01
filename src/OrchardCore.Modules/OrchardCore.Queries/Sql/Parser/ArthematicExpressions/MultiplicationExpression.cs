namespace OrchardCore.Queries.Sql.Parser
{
    public class MultiplicationExpression : BinaryExpression
    {
        public MultiplicationExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsDecimal() * Right.EvaluateAsDecimal();
    }
}
