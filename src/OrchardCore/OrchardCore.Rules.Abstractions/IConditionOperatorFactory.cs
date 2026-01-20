namespace OrchardCore.Rules
{
    public interface IConditionOperatorFactory
    {
        string Name { get; }
        ConditionOperator Create();
    }

    public class ConditionOperatorFactory<TConditionOperator> : IConditionOperatorFactory where TConditionOperator : ConditionOperator, new()
    {
        private static readonly string _typeName = typeof(TConditionOperator).Name;
        public string Name => _typeName;

        public ConditionOperator Create()
            => new TConditionOperator();
    }
}
