namespace OrchardCore.Workflows.Models
{
    /// <summary>
    /// Represents a transition between two activities.
    /// </summary>
    public class TransitionRecord
    {
        public int Id { get; set; }

        /// <summary>
        /// The source <see cref="ActivityRecord.Id"/>
        /// </summary>
        public int SourceActivityId { get; set; }

        /// <summary>
        /// The name of the outcome on the source <see cref="ActivityRecord"/>
        /// </summary>
        public string SourceOutcomeName { get; set; }

        /// <summary>
        /// The destination <see cref="ActivityRecord.Id"/>
        /// </summary>
        public int DestinationActivityId { get; set; }
    }
}