namespace OrchardCore.Rules.Models
{
    public class CultureCondition : Condition
    {
        public string Value { get; set; }
        public StringOperator Operation { get; set; }
    }
}
