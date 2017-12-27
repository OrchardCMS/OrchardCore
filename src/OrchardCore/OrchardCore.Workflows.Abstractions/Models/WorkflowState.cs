using System.Collections.Generic;

namespace OrchardCore.Workflows.Models
{
    /// <summary>
    /// Represents a workflow's runtime state.
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
        public IDictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// A dictionary of values provided by the initiator of the workflow.
        /// </summary>
        public IDictionary<string, object> Input { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// A dictionary of values that individual activities within a running workflow can return to the initiator of the workflow.
        /// </summary>
        public IDictionary<string, object> Output { get; set; } = new Dictionary<string, object>();
    }
}