using System.Collections.Generic;
using OrchardCore.Entities;

namespace OrchardCore.Workflows.Models
{
    /// <summary>
    /// Represents a workflow instance.
    /// </summary>
    public class WorkflowInstanceRecord
    {
        public int Id { get; set; }

        /// <summary>
        /// The ID of the workflow definition.
        /// </summary>
        public int DefinitionId { get; set; }

        /// <summary>
        /// Serialized state of the workflow.
        /// </summary>
        public IEntity State { get; set; } = new Entity();

        /// <summary>
        /// List of activities the current workflow instance is waiting on 
        /// for continuing its process.
        /// </summary>
        public IList<AwaitingActivityRecord> AwaitingActivities { get; } = new List<AwaitingActivityRecord>();
    }
}
