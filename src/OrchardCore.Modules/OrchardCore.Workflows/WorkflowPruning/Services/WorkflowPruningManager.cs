using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.WorkflowPruning.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Workflows.WorkflowPruning.Services;

public class WorkflowPruningManager : IWorkflowPruningManager
{
    private readonly ISiteService _siteService;
    private readonly ISession _session;
    private readonly IClock _clock;

    public WorkflowPruningManager(ISiteService siteService, ISession session, IClock clock)
    {
        _siteService = siteService;
        _session = session;
        _clock = clock;
    }

    public async Task<int> PruneWorkflowInstancesAsync(TimeSpan retentionPeriod)
    {
        var dateThreshold = _clock.UtcNow.AddDays(1) - retentionPeriod;
        var settings =  await _siteService.GetSiteSettingsAsync<WorkflowPruningSettings>();

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
