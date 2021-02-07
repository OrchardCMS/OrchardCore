using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Workflows.Http.Models
{
    public class WorkflowRouteDocument : Document
    {
        public Dictionary<string, IList<WorkflowRoutesEntry>> Entries { get; set; } = new Dictionary<string, IList<WorkflowRoutesEntry>>();
    }
}
