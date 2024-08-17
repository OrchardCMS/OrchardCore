using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Trimming.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Workflows.Trimming.Services;

public class WorkflowTrimmingService : IWorkflowTrimmingService
{
    private readonly ISiteService _siteService;
    private readonly ISession _session;
    private readonly IClock _clock;

    public WorkflowTrimmingService(
        ISiteService siteService,
        ISession session,
        IClock clock)
    {
        _siteService = siteService;
        _session = session;
        _clock = clock;
    }

    public async Task<int> TrimWorkflowInstancesAsync(TimeSpan retentionPeriod, int batchSize)
    {
        var dateThreshold = _clock.UtcNow - retentionPeriod;
        var settings = await _siteService.GetSettingsAsync<WorkflowTrimmingSettings>();

        settings.Statuses ??=
        [
            WorkflowStatus.Idle,
            WorkflowStatus.Starting,
            WorkflowStatus.Resuming,
            WorkflowStatus.Executing,
            WorkflowStatus.Halted,
            WorkflowStatus.Finished,
            WorkflowStatus.Faulted,
            WorkflowStatus.Aborted
        ];

        if (settings.Statuses.Length <= 0)
        {
            return 0;
        }

        var statuses = settings.Statuses.Select(x => (int)x).ToArray();
        var workflowInstances = await _session
            .Query<Workflow, WorkflowIndex>(x => x.WorkflowStatus.IsIn(statuses) && x.CreatedUtc <= dateThreshold)
            .OrderBy(x => x.Id)
            .Take(batchSize)
            .ListAsync();

        var total = 0;
        foreach (var item in workflowInstances)
        {
            _session.Delete(item);
            total++;
        }

        return total;
    }
}
