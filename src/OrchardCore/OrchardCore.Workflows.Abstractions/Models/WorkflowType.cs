using System.Collections.Generic;

namespace OrchardCore.Workflows.Models
{
    /// <summary>
    /// Represents a workflow type.
    /// </summary>
    public class WorkflowType
    {
        public long Id { get; set; }

        /// <summary>
        /// A unique identifier for this workflow type.
        /// </summary>
        public string WorkflowTypeId { get; set; }

        /// <summary>
        /// The name of this workflow type.
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
        /// The timeout in milliseconds to acquire a lock before resuming a given workflow instance of this type.
        /// </summary>
        public int LockTimeout { get; set; }

        /// <summary>
        /// The expiration in milliseconds of the lock acquired before resuming a workflow instance of this type.
        /// </summary>
        public int LockExpiration { get; set; }

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
