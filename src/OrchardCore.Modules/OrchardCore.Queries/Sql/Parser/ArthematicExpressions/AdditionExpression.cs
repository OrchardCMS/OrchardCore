namespace OrchardCore.Queries.Sql.Parser
{
    public class AdditionExpression : BinaryExpression
    {
        public AdditionExpression(Expression left, Expression right) : base(left, right)
        {

        }

        public override decimal Evaluate() => Left.Evaluate() + Right.Evaluate();
    }
}
