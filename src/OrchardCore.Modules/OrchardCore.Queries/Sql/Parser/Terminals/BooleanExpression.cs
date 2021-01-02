namespace OrchardCore.Queries.Sql.Parser.Expressions.Terminals
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
