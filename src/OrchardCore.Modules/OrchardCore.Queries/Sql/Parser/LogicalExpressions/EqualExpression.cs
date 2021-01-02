namespace OrchardCore.Queries.Sql.Parser.Expressions
{
    public class EqualExpression : BinaryExpression
    {
        public EqualExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsBoolean() == Right.EvaluateAsBoolean();
    }
}
