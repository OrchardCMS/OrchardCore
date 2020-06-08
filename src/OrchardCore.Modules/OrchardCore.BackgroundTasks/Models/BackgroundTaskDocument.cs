using System.Collections.Generic;

namespace OrchardCore.BackgroundTasks.Models
{
    public class BackgroundTaskDocument
    {
        public Dictionary<string, BackgroundTaskSettings> Settings { get; } = new Dictionary<string, BackgroundTaskSettings>();
    }
}
