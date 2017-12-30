using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowInstanceRepository
    {
        Task<IList<WorkflowInstanceRecord>> GetWaitingWorkflowInstancesAsync(string activityName, string correlationId = null);
        void Save(WorkflowInstanceRecord workflowInstance);
        void Delete(WorkflowInstanceRecord workflowInstance);
    }

    public static class WorkflowInstanceRepositoryExtensions
    {
        public static void Save(this IWorkflowInstanceRepository repository, WorkflowContext workflowContext)
        {
            workflowContext.WorkflowInstance.State = JObject.FromObject(workflowContext.State);
        }
    }
}
