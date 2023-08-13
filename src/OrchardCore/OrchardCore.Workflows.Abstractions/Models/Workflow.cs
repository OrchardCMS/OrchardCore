using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Workflows.Models
{
    /// <summary>
    /// Represents a workflow instance.
    /// </summary>
    public class Workflow
    {
        public long Id { get; set; }

        /// <summary>
        /// A unique identifier for this workflow.
        /// </summary>
        public string WorkflowId { get; set; }

        public string WorkflowTypeId { get; set; }

        /// <summary>
        /// The correlation ID can be used to resume workflows that are associated with specific objects, such as content items.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Serialized state of the workflow.
        /// </summary>
        public JObject State { get; set; } = new JObject();

        public WorkflowStatus Status { get; set; }
        public string FaultMessage { get; set; }

        /// <summary>
        /// The timeout in milliseconds to acquire a lock before resuming this workflow instance.
        /// </summary>
        public int LockTimeout { get; set; }

        /// <summary>
        /// The expiration in milliseconds of the lock acquired before resuming this workflow instance.
        /// </summary>
        public int LockExpiration { get; set; }

        /// <summary>
        /// List of activities the current workflow instance is waiting on
        /// for continuing its process.
        /// </summary>
        public IList<BlockingActivity> BlockingActivities { get; } = new List<BlockingActivity>();

        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Whether this workflow instance needs to be resumed atomically.
        /// </summary>
        public bool IsAtomic => LockTimeout > 0 && LockExpiration > 0;
    }
}
