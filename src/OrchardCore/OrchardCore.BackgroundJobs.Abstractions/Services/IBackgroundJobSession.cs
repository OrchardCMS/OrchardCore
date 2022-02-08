using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs.Services
{
    public interface IBackgroundJobSession
    {
        void Store(BackgroundJobExecution backgroundJobExecution);
        bool TryRecallBackgroundJobById(string backgroundJobId, out BackgroundJobExecution backgroundJobExecution);
        bool TryRecallBackgroundJobByCorrelationId(string correlationId, out BackgroundJobExecution backgroundJobExecution);
    }
}
