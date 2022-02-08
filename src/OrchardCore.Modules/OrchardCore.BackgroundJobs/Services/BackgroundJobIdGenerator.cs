using OrchardCore.Entities;

namespace OrchardCore.BackgroundJobs.Services
{
    public class BackgroundJobIdGenerator : IBackgroundJobIdGenerator
    {
        private readonly IIdGenerator _idGenerator;

        public BackgroundJobIdGenerator(IIdGenerator idGenerator) => _idGenerator = idGenerator;

        public string GenerateUniqueId(IBackgroundJob backgroundJob)
            => _idGenerator.GenerateUniqueId();
    }
}
