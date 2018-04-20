using OrchardCore.Entities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowDefinitionIdGenerator : IWorkflowDefinitionIdGenerator
    {
        private readonly IIdGenerator _idGenerator;

        public WorkflowDefinitionIdGenerator(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public string GenerateUniqueId(WorkflowDefinition workflowDefinitionecord)
        {
            return _idGenerator.GenerateUniqueId();
        }
    }
}
