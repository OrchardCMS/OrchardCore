namespace OrchardCore.Rules
{
    public interface IConditionOperatorResolver
    {
        IOperatorComparer GetOperatorComparer(ConditionOperator conditionOperator);
    }
}
