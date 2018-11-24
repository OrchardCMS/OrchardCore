using System.Collections.Generic;

namespace OrchardCore.BackgroundTasks.Models
{
    public class BackgroundTaskDocument
    {
        public int Id { get; set; } // An identifier so that updates don't create new documents

        public Dictionary<string, BackgroundTaskSettings> Settings { get; } = new Dictionary<string, BackgroundTaskSettings>();
    }
}
