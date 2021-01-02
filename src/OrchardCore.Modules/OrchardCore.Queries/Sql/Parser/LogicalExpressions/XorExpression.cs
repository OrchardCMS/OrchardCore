namespace OrchardCore.Queries.Sql.Parser.Expressions
{
    public class XorExpression : BinaryExpression
    {
        public XorExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsBoolean() ^ Right.EvaluateAsBoolean();
    }
}
