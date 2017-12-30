using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using YesSql;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowInstanceRepository : IWorkflowInstanceRepository
    {
        private readonly ISession _session;

        public WorkflowInstanceRepository(ISession session)
        {
            _session = session;
        }

        public async Task<IList<WorkflowInstanceRecord>> GetWaitingWorkflowInstancesAsync(string activityName, string correlationId = null)
        {
            var query = await _session
                .QueryIndex<WorkflowInstanceByAwaitingActivitiesIndex>(index =>
                    index.ActivityName == activityName &&
                    index.ActivityIsStart == false &&
                    index.WorkflowInstanceCorrelationId == correlationId)
                .ListAsync();

            var pendingWorkflowInstanceIndexes = query.ToList();
            var pendingWorkflowInstanceIds = pendingWorkflowInstanceIndexes.Select(x => x.WorkflowInstanceId).Distinct().ToArray();
            var pendingWorkflowInstances = await _session.GetAsync<WorkflowInstanceRecord>(pendingWorkflowInstanceIds);

            return pendingWorkflowInstances.ToList();
        }

        public void Save(WorkflowInstanceRecord workflowInstance)
        {
            _session.Save(workflowInstance);
        }

        public void Delete(WorkflowInstanceRecord workflowInstance)
        {
            _session.Delete(workflowInstance);
        }
    }
}
