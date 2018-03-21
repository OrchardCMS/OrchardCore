using System.Collections.Generic;

namespace OrchardCore.BackgroundTasks.Models
{
    public class BackgroundTaskDocument
    {
        public int Id { get; set; } // An identifier so that updates don't create new documents

        public Dictionary<string, BackgroundTask> Tasks { get; } = new Dictionary<string, BackgroundTask>();
    }

    public class BackgroundTask
    {
        public string Name { get; set; }
        public bool Enable { get; set; }
        public string Schedule { get; set; }
        public string Description { get; set; }
    }
}
