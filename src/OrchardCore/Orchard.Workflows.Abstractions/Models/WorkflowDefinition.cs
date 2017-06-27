using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Workflows.Models {
    /// <summary>
    /// Represent a workflow definition comprised of activities and transitions between them.
    /// </summary>
    public class WorkflowDefinition {
        public int Id { get; set; }

        /// <summary>
        /// Whether or not to enable workflows of this type.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The name of the workflow definition.
        /// </summary>
        [Required, StringLength(1024)]
        public string Name { get; set; }

        /// <summary>
        /// List of <see cref="Activity"/> composing this workflow definition.
        /// </summary>
        public IList<Activity> Activities { get; set; } = new List<Activity>();

        /// <summary>
        /// List of <see cref="Transition"/> composing this workflow definition.
        /// This is distinct from Activities as there could be activities without 
        /// any connection an any time of the design process, though they should
        /// be synchronized.
        /// </summary>
        public IList<Transition> Transitions { get; set; } = new List<Transition>();

        /// <summary>
        /// List of <see cref="Workflow"/> associated with this workflow definition.
        /// </summary>
        public IList<Workflow> Workflows { get; set; } = new List<Workflow>();
    }
}