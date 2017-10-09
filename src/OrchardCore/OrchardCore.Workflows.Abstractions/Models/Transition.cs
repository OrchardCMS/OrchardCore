namespace OrchardCore.Workflows.Models
{
    /// <summary>
    /// Represents a transition between two <see cref="Activity"/> objects.
    /// </summary>
    public class Transition
    {
        public int Id { get; set; }

        /// <summary>
        /// The source <see cref="Activity.Id"/>
        /// </summary>
        public int SourceActivityId { get; set; }

        /// <summary>
        /// The name of the endpoint on the source <see cref="Activity"/>
        /// </summary>
        public string SourceEndpoint { get; set; }

        /// <summary>
        /// The destination <see cref="Activity.Id"/>
        /// </summary>
        public int DestinationActivityId { get; set; }

        /// <summary>
        /// The name of the endpoint on the destination <see cref="Activity"/>
        /// </summary>
        public string DestinationEndpoint { get; set; }
    }
}