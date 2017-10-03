namespace Orchard.Workflows.Models {
    /// <summary>
    /// Reprensents a transition between two <see cref="Activity"/>
    /// </summary>
    public class Transition {

        public int Id { get; set; }

        /// <summary>
        /// The source <see cref="Activity.Id"/>
        /// </summary>
        public string SourceActivityId { get; set; }

        /// <summary>
        /// The name of the endpoint on the source <see cref="Activity"/>
        /// </summary>
        public string SourceEndpoint { get; set; }

        /// <summary>
        /// The destination <see cref="Activity.Id"/>
        /// </summary>
        public string DestinationActivityId { get; set; }

        /// <summary>
        /// The name of the endpoint on the destination <see cref="Activity"/>
        /// </summary>
        public string DestinationEndpoint { get; set; }
    }
}