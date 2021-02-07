using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public interface IRuleFactory
    {
        string Name { get; }
        Rule Create();
    }

    public class RuleFactory<TRule> : IRuleFactory where TRule : Rule, new()
    {
        private static readonly string TypeName = typeof(TRule).Name;
        public string Name => TypeName;

        public Rule Create()
            => new Rule();     
    }

}
