namespace OrchardCore.Queries.Sql.Parser
{
    public class ModulusExpression : BinaryExpression
    {
        public ModulusExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public override object Evaluate() => Left.EvaluateAsNumber() % Right.EvaluateAsNumber();
    }
}
