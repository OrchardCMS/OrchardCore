namespace OrchardCore.Queries.Sql.Parser
{
    public class NumberExpression : Expression
    {
        public NumberExpression(decimal value)
        {
            Value = value;
        }

        public decimal Value { get; set; }

        public override object Evaluate() => Value;
    }
}
