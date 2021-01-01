namespace OrchardCore.Queries.Sql.Parser
{
    public class BooleanExpression : Expression
    {
        public BooleanExpression(bool value)
        {
            Value = value;
        }

        public bool Value { get; set; }

        public override object Evaluate() => Value;
    }
}
