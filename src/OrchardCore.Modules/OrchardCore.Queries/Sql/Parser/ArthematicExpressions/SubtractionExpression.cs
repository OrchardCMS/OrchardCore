namespace OrchardCore.Queries.Sql.Parser
{
    public class SubtractionExpression : BinaryExpression
    {
        public SubtractionExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsDecimal() - Right.EvaluateAsDecimal();
    }
}
