using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs.Services
{
    public interface IBackgroundJobStore
    {
        ValueTask CreateJobAsync(BackgroundJobExecution backgroundJobExecution);
        ValueTask<bool> DeleteJobAsync(BackgroundJobExecution backgroundJobExecution);
        ValueTask<BackgroundJobExecution> GetJobByCorrelationIdAsync(string correlationId);
        ValueTask<BackgroundJobExecution> GetJobByIdAsync(string backgroundJobId);
        ValueTask<IEnumerable<BackgroundJobExecution>> GetJobsByIdAsync(string[] backgroundJobIds);
        ValueTask UpdateJobAsync(BackgroundJobExecution backgroundJobExecution);
    }
}
