using OrchardCore.Entities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowInstanceIdGenerator : IWorkflowInstanceIdGenerator
    {
        private readonly IIdGenerator _idGenerator;

        public WorkflowInstanceIdGenerator(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public string GenerateUniqueId(WorkflowInstance workflowInstanceRecord)
        {
            return _idGenerator.GenerateUniqueId();
        }
    }
}
