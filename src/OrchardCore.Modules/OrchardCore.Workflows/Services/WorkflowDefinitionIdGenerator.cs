using OrchardCore.Entities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowDefinitionIdGenerator : IWorkflowTypeIdGenerator
    {
        private readonly IIdGenerator _idGenerator;

        public WorkflowDefinitionIdGenerator(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public string GenerateUniqueId(WorkflowType workflowDefinitionecord)
        {
            return _idGenerator.GenerateUniqueId();
        }
    }
}
