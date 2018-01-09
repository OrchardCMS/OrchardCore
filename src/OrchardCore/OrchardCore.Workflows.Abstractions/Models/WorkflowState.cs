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
        public Stack<object> Stack { get; set; } = new Stack<object>();

        /// <summary>
        /// A dictionary of values that activities within a running workflow can read and write information from and to.
        /// </summary>
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}