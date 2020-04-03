using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IActivityIdGenerator
    {
        string GenerateUniqueId(ActivityRecord activityRecord);
    }
}
