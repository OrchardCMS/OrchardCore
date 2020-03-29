using System.Collections.Generic;

namespace OrchardCore.Workflows.ViewModels
{
    public class ActivityViewModel
    {
        /// <summary>
        /// The local id used for connections
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The name of the activity
        /// </summary>
        public string Name { get; set; }

        public IDictionary<string, string> State { get; set; }
    }
}
