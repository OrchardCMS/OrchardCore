namespace OrchardCore.Queries.Sql.Parser.Expressions
{
    public class GreaterExpression : BinaryExpression
    {
        public GreaterExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsDecimal() > Right.EvaluateAsDecimal();
    }
}
