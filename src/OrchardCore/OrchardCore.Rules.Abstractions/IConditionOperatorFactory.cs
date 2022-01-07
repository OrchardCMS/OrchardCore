namespace OrchardCore.Rules
{
    public interface IConditionOperatorFactory
    {
        string Name { get; }
        ConditionOperator Create();
    }

    public class ConditionOperatorFactory<TConditionOperator> : IConditionOperatorFactory where TConditionOperator : ConditionOperator, new()
    {
        private static readonly string TypeName = typeof(TConditionOperator).Name;
        public string Name => TypeName;

        public ConditionOperator Create()
            => new TConditionOperator();
    }
}
