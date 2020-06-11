namespace OrchardCore.Workflows.Models
{
    /// <summary>
    /// Represents a transition between two activities.
    /// </summary>
    public class Transition
    {
        public int Id { get; set; }

        /// <summary>
        /// The source <see cref="ActivityRecord.ActivityId"/>
        /// </summary>
        public string SourceActivityId { get; set; }

        /// <summary>
        /// The name of the outcome on the source <see cref="ActivityRecord"/>
        /// </summary>
        public string SourceOutcomeName { get; set; }

        /// <summary>
        /// The destination <see cref="ActivityRecord.ActivityId"/>
        /// </summary>
        public string DestinationActivityId { get; set; }
    }
}
