using OrchardCore.Entities;

namespace OrchardCore.Setup.Services
{
    public class SetupUserIdGenerator : ISetupUserIdGenerator
    {
        private readonly IIdGenerator _generator;

        public SetupUserIdGenerator(IIdGenerator generator)
        {
            _generator = generator;
        }

        public string GenerateUniqueId()
        {
            return _generator.GenerateUniqueId();
        }
    }
}
