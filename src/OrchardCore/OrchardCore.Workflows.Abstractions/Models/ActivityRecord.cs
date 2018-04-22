using OrchardCore.Entities;

namespace OrchardCore.Workflows.Models
{
    public class ActivityRecord : Entity
    {
        public string ActivityId { get; set; }

        /// <summary>
        /// The type of the activity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The left coordinate of the activity.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The top coordinate of the activity.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Whether the activity is a start state for a WorkflowDefinition.
        /// </summary>
        public bool IsStart { get; set; }
    }
}