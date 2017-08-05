using System.Collections.Generic;

namespace Orchard.Workflows.Models
{
    /// <summary>
    /// Reprensents a running workflow instance.
    /// </summary>
    public class Workflow {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Enabled { get; set; }

        /// <summary>
        /// List of activities the current workflow instance is waiting on 
        /// for continuing its process.
        /// </summary>
        public IList<Activity> Activities { get; set; } = new List<Activity>();

        /// <summary>
        /// List of transitions the current workflow instance is waiting on 
        /// for continuing its process.
        /// </summary>
        public IList<Transition> Transitions { get; set; } = new List<Transition>();
    }
}