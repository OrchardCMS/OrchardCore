using OrchardCore.Entities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowInstanceIdGenerator : IWorkflowIdGenerator
    {
        private readonly IIdGenerator _idGenerator;

        public WorkflowInstanceIdGenerator(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public string GenerateUniqueId(Workflow workflowInstanceRecord)
        {
            return _idGenerator.GenerateUniqueId();
        }
    }
}
