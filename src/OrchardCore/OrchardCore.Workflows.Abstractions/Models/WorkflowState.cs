using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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

        /// <summary>
        /// A dictionary of activity states. Each entry contains runtime state for a particular activity.
        /// </summary>
        public IDictionary<string, JObject> ActivityStates { get; set; } = new Dictionary<string, JObject>();

        /// <summary>
        /// The list of executed activities.
        /// </summary>
        public IList<ExecutedActivity> ExecutedActivities { get; set; } = new List<ExecutedActivity>();
    }
}
