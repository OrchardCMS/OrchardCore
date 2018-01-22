using System.Collections.Generic;

namespace OrchardCore.Workflows.Models
{
    /// <summary>
    /// Represents a workflow's serializable runtime state.
    /// </summary>
    public class WorkflowState
    {
        /// <summary>
        /// The stack of values pushed onto by individual activities.
        /// Can be used by subsequent activities to pop the last value and do something with it.
        /// </summary>
        public object LastResult { get; set; }

        /// <summary>
        /// A dictionary of input values provided by the caller of the workflow.
        /// </summary>
        public IDictionary<string, object> Input { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// A dictionary of output values provided by activities of the workflow.
        /// </summary>
        public IDictionary<string, object> Output { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// A dictionary of values that activities within a running workflow can read and write information from and to.
        /// </summary>
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public IList<int> ExecutedActivities { get; set; } = new List<int>();
    }
}