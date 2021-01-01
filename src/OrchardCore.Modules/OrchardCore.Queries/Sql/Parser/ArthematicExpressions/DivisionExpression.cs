namespace OrchardCore.Queries.Sql.Parser
{
    public class DivisionExpression : BinaryExpression
    {
        public DivisionExpression(Expression left, Expression right) : base(left, right)
        {

        }

        public override decimal Evaluate() => Left.Evaluate() / Right.Evaluate();
    }
}
