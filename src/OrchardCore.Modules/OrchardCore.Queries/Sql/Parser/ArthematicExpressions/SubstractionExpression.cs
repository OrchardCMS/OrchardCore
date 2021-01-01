namespace OrchardCore.Queries.Sql.Parser
{
    public class SubstractionExpression : BinaryExpression
    {
        public SubstractionExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsDecimal() - Right.EvaluateAsDecimal();
    }
}
