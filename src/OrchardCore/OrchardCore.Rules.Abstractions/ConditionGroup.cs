namespace OrchardCore.Rules;

public class ConditionGroup : Condition
{
    public List<Condition> Conditions { get; init; } = [];
}

public abstract class DisplayTextConditionGroup : ConditionGroup
{
    public string DisplayText { get; set; }
}
