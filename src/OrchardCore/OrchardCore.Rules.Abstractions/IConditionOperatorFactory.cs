namespace OrchardCore.Rules;

public interface IConditionOperatorFactory
{
    string Name { get; }
    ConditionOperator Create();
}

public class ConditionOperatorFactory<TConditionOperator> : IConditionOperatorFactory where TConditionOperator : ConditionOperator, new()
{
    private static readonly string s_typeName = typeof(TConditionOperator).Name;
    public string Name => s_typeName;

    public ConditionOperator Create()
        => new TConditionOperator();
}
