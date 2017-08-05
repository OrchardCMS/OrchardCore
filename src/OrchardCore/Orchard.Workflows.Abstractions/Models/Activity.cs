using Newtonsoft.Json;

namespace Orchard.Workflows.Models
{
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
        /// The parent <see cref="Workflow"/> 
        /// containing this activity.
        /// </summary>
        [JsonIgnore]
        public int WorkflowDefinitionId { get; set; }

        /// <summary>
        /// Gets the Id which can be used on the client. 
        /// </summary>
        /// <returns>An unique Id to represent this activity on the client.</returns>
        public string ClientId => string.Format("{0}_{1}", Name, Id);
    }
}