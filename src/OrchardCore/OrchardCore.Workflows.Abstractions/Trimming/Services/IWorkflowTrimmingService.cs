namespace OrchardCore.Workflows.Trimming.Services;

public interface IWorkflowTrimmingService
{
    Task<int> TrimWorkflowInstancesAsync(TimeSpan retentionPeriod, int batchSize);
}
