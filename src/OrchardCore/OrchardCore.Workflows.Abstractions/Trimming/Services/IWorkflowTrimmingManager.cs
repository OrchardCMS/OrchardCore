using System;
using System.Threading.Tasks;

namespace OrchardCore.Workflows.Trimming.Services;

public interface IWorkflowTrimmingManager
{
    Task<int> TrimWorkflowInstancesAsync(TimeSpan retentionPeriod, int batchSize);
}
