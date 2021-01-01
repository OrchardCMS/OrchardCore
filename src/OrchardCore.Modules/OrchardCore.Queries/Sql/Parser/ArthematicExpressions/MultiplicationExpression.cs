namespace OrchardCore.Queries.Sql.Parser
{
    public class MultiplicationExpression : BinaryExpression
    {
        public MultiplicationExpression(Expression left, Expression right) : base(left, right)
        {

        }

        public override decimal Evaluate() => Left.Evaluate() * Right.Evaluate();
    }
}
