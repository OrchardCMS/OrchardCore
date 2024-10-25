namespace OrchardCore.Rules.Services;

public interface IConditionIdGenerator
{
    void GenerateUniqueId(Condition condition);
}
