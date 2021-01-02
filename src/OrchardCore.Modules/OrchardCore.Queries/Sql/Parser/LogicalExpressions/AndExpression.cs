namespace OrchardCore.Queries.Sql.Parser.Expressions
{
    public class AndExpression : BinaryExpression
    {
        public AndExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsBoolean() && Right.EvaluateAsBoolean();
    }
}
