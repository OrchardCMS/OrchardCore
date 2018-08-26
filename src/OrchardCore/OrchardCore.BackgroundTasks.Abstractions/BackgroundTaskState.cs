using System;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskState
    {
        public static BackgroundTaskState Undefined = new BackgroundTaskState();

        public string Name { get; set; }
        public DateTime LastStartTime { get; set; }
        public DateTime NextStartTime { get; set; }
    }
}
