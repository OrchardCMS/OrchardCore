namespace OrchardCore.Queries.Sql.Parser.Expressions.Terminals
{
    public class IdentifierExpression : Expression
    {
        public IdentifierExpression(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override object Evaluate() => Name;
    }
}
