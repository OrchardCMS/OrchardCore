using System.Collections.Generic;

namespace OrchardCore.BackgroundTasks.Models
{
    public class BackgroundTaskDocument
    {
        public int Id { get; set; } // An identifier so that updates don't create new documents

        public Dictionary<string, BackgroundTaskDefinition> Tasks { get; } = new Dictionary<string, BackgroundTaskDefinition>();
    }

    public class BackgroundTaskDefinition : BackgroundTaskSettings
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
