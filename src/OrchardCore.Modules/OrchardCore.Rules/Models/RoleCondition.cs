namespace OrchardCore.Rules.Models
{
    public class RoleCondition : Condition
    {
        public string Value { get; set; }
        public StringOperator Operation { get; set; }
    }
}
