using System;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskState : BackgroundTaskSettings
    {
        public static BackgroundTaskState Empty = new BackgroundTaskState() { Enable = false };

        public string Name { get; set; }

        public DateTime LastStartTime { get; set; }
        public virtual DateTime NextStartTime { get; set; }
        public TimeSpan LastExecutionTime { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }

        public int StartCount { get; set; }
        public string FaultMessage { get; set; }
        public BackgroundTaskStatus Status { get; set; }
    }
}
