namespace OrchardCore.Rules.Models
{
    public class ContentTypeCondition : Condition
    {
        public string Value { get; set; }
        public StringOperator Operation { get; set; }
    }
}
