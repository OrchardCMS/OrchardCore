namespace OrchardCore.Rules
{
    public interface IConditionFactory
    {
        string Name { get; }
        Condition Create();
    }

    public class ConditionFactory<TCondition> : IConditionFactory where TCondition : Condition, new()
    {
        private static readonly string _typeName = typeof(TCondition).Name;
        public string Name => _typeName;

        public Condition Create()
            => new TCondition()
            {
                Name = Name,
            };
    }
}
