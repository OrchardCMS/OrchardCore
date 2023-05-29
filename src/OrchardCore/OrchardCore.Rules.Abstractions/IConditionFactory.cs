namespace OrchardCore.Rules
{
    public interface IConditionFactory
    {
        string Name { get; }
        Condition Create();
    }

    public class ConditionFactory<TCondition> : IConditionFactory where TCondition : Condition, new()
    {
        private static readonly string TypeName = typeof(TCondition).Name;
        public string Name => TypeName;

        public Condition Create()
            => new TCondition()
            {
                Name = this.Name
            };
    }
}
