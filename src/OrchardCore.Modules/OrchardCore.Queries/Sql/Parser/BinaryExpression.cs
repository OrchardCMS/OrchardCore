namespace OrchardCore.Queries.Sql.Parser
{
    public abstract class BinaryExpression : Expression
    {
        public BinaryExpression(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        public Expression Left { get; set; }

        public Expression Right { get; set; }

    }
}
