namespace OrchardCore.Queries.Sql.Parser
{
    public class StringExpression : Expression
    {
        public StringExpression(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public override object Evaluate() => Value;
    }
}
