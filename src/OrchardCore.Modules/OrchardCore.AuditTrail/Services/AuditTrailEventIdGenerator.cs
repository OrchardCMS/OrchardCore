using OrchardCore.Entities;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailEventIdGenerator : IAuditTrailEventIdGenerator
    {
        private readonly IIdGenerator _generator;

        public AuditTrailEventIdGenerator(IIdGenerator generator)
        {
            _generator = generator;
        }

        public string GenerateUniqueId() => _generator.GenerateUniqueId();
    }
}
