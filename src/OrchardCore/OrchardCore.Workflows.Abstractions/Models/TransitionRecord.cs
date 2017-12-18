namespace OrchardCore.Workflows.Models
{
    /// <summary>
    /// Represents a transition between two <see cref="ActivityRecord"/> objects.
    /// </summary>
    public class TransitionRecord
    {
        public int Id { get; set; }

        /// <summary>
        /// The source <see cref="ActivityRecord.Id"/>
        /// </summary>
        public int SourceActivityId { get; set; }

        /// <summary>
        /// The name of the endpoint on the source <see cref="ActivityRecord"/>
        /// </summary>
        public string SourceEndpoint { get; set; }

        /// <summary>
        /// The destination <see cref="ActivityRecord.Id"/>
        /// </summary>
        public int DestinationActivityId { get; set; }

        /// <summary>
        /// The name of the endpoint on the destination <see cref="ActivityRecord"/>
        /// </summary>
        public string DestinationEndpoint { get; set; }
    }
}