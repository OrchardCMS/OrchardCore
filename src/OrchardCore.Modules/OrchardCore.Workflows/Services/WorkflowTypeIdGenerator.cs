using OrchardCore.Entities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowTypeIdGenerator : IWorkflowTypeIdGenerator
    {
        private readonly IIdGenerator _idGenerator;

        public WorkflowTypeIdGenerator(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public string GenerateUniqueId(WorkflowType workflowType)
        {
            return _idGenerator.GenerateUniqueId();
        }
    }
}
