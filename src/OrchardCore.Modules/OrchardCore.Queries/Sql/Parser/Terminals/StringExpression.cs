namespace OrchardCore.Queries.Sql.Parser.Expressions.Terminals
{
    public class StringExpression : Expression
    {
        public StringExpression(string value, StringQuote quoteType = StringQuote.Double)
        {
            Value = value;
            StringQuoteType = quoteType;
        }

        public StringQuote StringQuoteType { get; }

        public string Value { get; }

        public override object Evaluate() => Value;
    }

    public enum StringQuote
    {
        Single = 1,
        Double = 0
    }
}
