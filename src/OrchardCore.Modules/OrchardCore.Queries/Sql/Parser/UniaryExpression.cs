namespace OrchardCore.Queries.Sql.Parser
{
    public abstract class UnaryExpression : Expression
    {
        public UnaryExpression(Expression expression)
        {
            InnerExpression = expression;
        }

        public Expression InnerExpression { get; set; }
    }
}
