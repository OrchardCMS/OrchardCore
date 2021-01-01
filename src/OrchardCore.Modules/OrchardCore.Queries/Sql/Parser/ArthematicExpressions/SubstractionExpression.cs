namespace OrchardCore.Queries.Sql.Parser
{
    public class SubstractionExpression : BinaryExpression
    {
        public SubstractionExpression(Expression left, Expression right) : base(left, right)
        {

        }

        public override decimal Evaluate() => Left.Evaluate() - Right.Evaluate();
    }
}
