using System.Collections.Generic;

namespace Orchard.Workflows.Models {
    /// <summary>
    /// Reprensents a running workflow instance.
    /// </summary>
    public class Workflow {
        public int Id { get; set; }

        /// <summary>
        /// Serialized state of the workflow.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// List of activities the current workflow instance is waiting on 
        /// for continuing its process.
        /// </summary>
        public IList<AwaitingActivity> AwaitingActivities { get; set; } = new List<AwaitingActivity>();

        /// <summary>
        /// Parent <see cref="WorkflowDefinitionRecord"/>
        /// </summary>
        public WorkflowDefinition Definition { get; set; }
    }
}