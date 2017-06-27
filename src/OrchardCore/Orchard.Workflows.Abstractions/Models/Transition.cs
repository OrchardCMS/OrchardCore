namespace Orchard.Workflows.Models {
    /// <summary>
    /// Reprensents a transition between two <see cref="Activity"/>
    /// </summary>
    public class Transition {

        public int Id { get; set; }

        /// <summary>
        /// The source <see cref="Activity"/>
        /// </summary>
        public Activity SourceActivity { get; set; }

        /// <summary>
        /// The name of the endpoint on the source <see cref="Activity"/>
        /// </summary>
        public string SourceEndpoint { get; set; }

        /// <summary>
        /// The destination <see cref="Activity"/>
        /// </summary>
        public Activity DestinationActivity { get; set; }

        /// <summary>
        /// The name of the endpoint on the destination <see cref="Activity"/>
        /// </summary>
        public string DestinationEndpoint { get; set; }

        /// <summary>
        /// The parent <see cref="WorkflowDefinition"/>
        /// </summary>
        public WorkflowDefinition Definition { get; set; }
    }
}