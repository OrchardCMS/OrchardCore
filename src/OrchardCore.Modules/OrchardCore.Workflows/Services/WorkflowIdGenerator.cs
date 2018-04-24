using OrchardCore.Entities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowIdGenerator : IWorkflowIdGenerator
    {
        private readonly IIdGenerator _idGenerator;

        public WorkflowIdGenerator(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public string GenerateUniqueId(Workflow workflow)
        {
            return _idGenerator.GenerateUniqueId();
        }
    }
}
