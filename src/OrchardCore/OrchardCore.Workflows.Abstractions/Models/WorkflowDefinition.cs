using System.Collections.Generic;

namespace OrchardCore.Workflows.Models
{
    /// <summary>
    /// Represents a workflow definition.
    /// </summary>
    public class WorkflowDefinition
    {
        public int Id { get; set; }

        /// <summary>
        /// The name of this workflow.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether this workflow definition is enabled or not.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// A complete list of all activities that are part of this workflow.
        /// </summary>
        public IList<Activity> Activities { get; set; } = new List<Activity>();

        /// <summary>
        /// A complete list of the transitions between the activities on this workflow.
        /// </summary>
        public IList<Transition> Transitions { get; set; } = new List<Transition>();
    }
}