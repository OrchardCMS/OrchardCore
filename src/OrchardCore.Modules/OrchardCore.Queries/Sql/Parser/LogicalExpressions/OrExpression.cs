namespace OrchardCore.Queries.Sql.Parser.Expressions
{
    public class OrExpression : BinaryExpression
    {
        public OrExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsBoolean() || Right.EvaluateAsBoolean();
    }
}
