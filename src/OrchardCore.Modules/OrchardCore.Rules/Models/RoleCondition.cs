namespace OrchardCore.Rules.Models;

public class RoleCondition : Condition
{
    public string Value { get; set; }
    public ConditionOperator Operation { get; set; }
}
