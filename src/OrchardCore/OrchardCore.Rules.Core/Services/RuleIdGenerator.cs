using OrchardCore.Entities;

namespace OrchardCore.Rules.Services
{
    public class RuleIdGenerator : IRuleIdGenerator
    {
        private readonly IIdGenerator _idGenerator;

        public RuleIdGenerator(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public string GenerateUniqueId()
        {
            return _idGenerator.GenerateUniqueId();
        }
    }
}
