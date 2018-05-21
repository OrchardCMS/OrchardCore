using OrchardCore.Entities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class ActivityIdGenerator : IActivityIdGenerator
    {
        private readonly IIdGenerator _idGenerator;

        public ActivityIdGenerator(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public string GenerateUniqueId(ActivityRecord activityRecord)
        {
            return _idGenerator.GenerateUniqueId();
        }
    }
}
