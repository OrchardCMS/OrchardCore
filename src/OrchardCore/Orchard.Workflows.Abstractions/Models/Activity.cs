namespace Orchard.Workflows.Models
{
    /// <summary>
    /// Represents an activity in a <see cref="WorkflowDefinition"/>
    /// </summary>
    public class Activity
    {
        public int Id { get; set; }

        /// <summary>
        /// The type of the activity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The serialized state of the activity.
        /// </summary>
        public string State { get; set; }

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
        public bool Start { get; set; }

        /// <summary>
        /// The parent <see cref="WorkflowDefinition"/> 
        /// containing this activity.
        /// </summary>
        public WorkflowDefinition Definition { get; set; }

        /// <summary>
        /// Gets the Id which can be used on the client. 
        /// </summary>
        /// <returns>An unique Id to represent this activity on the client.</returns>
        public string GetClientId() => string.Format("{0}_{1}", Name, Id);
    }
}