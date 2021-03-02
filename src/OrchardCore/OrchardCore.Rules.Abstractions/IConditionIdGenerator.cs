namespace OrchardCore.Rules
{
    public interface IConditionIdGenerator
    {
        void GenerateUniqueId(Condition condition);
    }
}
