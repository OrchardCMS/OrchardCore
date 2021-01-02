namespace OrchardCore.Queries.Sql.Parser.Expressions
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
