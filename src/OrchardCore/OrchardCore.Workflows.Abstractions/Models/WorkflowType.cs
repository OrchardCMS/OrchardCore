using System.Collections.Generic;

namespace OrchardCore.Workflows.Models
{
    /// <summary>
    /// Represents a workflow type.
    /// </summary>
    public class WorkflowType
    {
        public int Id { get; set; }
        public string WorkflowTypeId { get; set; }

        /// <summary>
        /// The name of this workflow.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether this workflow definition is enabled or not.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Controls whether this workflow can spawn one or multiple instances.
        /// </summary>
        public bool IsSingleton { get; set; }

        /// <summary>
        /// Controls whether workflow instances will be deleted upon completion.
        /// </summary>
        public bool DeleteFinishedWorkflows { get; set; }

        /// <summary>
        /// A complete list of all activities that are part of this workflow.
        /// </summary>
        public IList<ActivityRecord> Activities { get; set; } = new List<ActivityRecord>();

        /// <summary>
        /// A complete list of the transitions between the activities on this workflow.
        /// </summary>
        public IList<Transition> Transitions { get; set; } = new List<Transition>();
    }
}
