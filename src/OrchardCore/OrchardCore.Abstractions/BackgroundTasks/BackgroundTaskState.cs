using System;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskState
    {
        public string Name { get; set; }
        public DateTime LastStartTime { get; set; }
    }
}
