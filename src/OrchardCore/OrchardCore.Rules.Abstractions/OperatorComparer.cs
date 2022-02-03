namespace OrchardCore.Rules
{
    public abstract class OperatorComparer<TConditionOperator, TCompare> : IOperatorComparer where TConditionOperator : ConditionOperator
    {
        bool IOperatorComparer.Compare(ConditionOperator conditionOperator, object a, object b)
            => Compare((TConditionOperator)conditionOperator, (TCompare)a, (TCompare)b);
        public abstract bool Compare(TConditionOperator conditionOperator, TCompare a, TCompare b);
    }
}
