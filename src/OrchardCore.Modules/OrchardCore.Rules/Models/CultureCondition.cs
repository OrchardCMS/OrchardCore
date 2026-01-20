namespace OrchardCore.Rules.Models;

public class CultureCondition : Condition
{
    public string Value { get; set; }
    public ConditionOperator Operation { get; set; }
}
