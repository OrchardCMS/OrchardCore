namespace OrchardCore.Rules.Models;

public class ContentTypeCondition : Condition
{
    public string Value { get; set; }
    public ConditionOperator Operation { get; set; }
}
