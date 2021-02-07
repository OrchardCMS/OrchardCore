namespace OrchardCore.Rules.Models
{
    public class UrlRule : Rule
    {
        public string Value { get; set; } 
        public StringOperator Operation { get; set; }
    }
}