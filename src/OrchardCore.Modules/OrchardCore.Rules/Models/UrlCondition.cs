namespace OrchardCore.Rules.Models
{
    public class UrlCondition : Condition
    {
        public string Value { get; set; }
        public StringOperator Operation { get; set; }
    }
}
