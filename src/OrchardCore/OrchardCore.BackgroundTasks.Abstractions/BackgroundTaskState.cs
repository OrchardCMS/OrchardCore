using System;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskState
    {
        public static BackgroundTaskState Undefined = new BackgroundTaskState();

        public string Name { get; set; }
        public int StartCount { get; set; }
        public DateTime LastStartTime { get; set; }
        public DateTime NextStartTime { get; set; }
        public BackgroundTaskStatus Status { get; set; }
        public string FaultMessage { get; set; }

        public BackgroundTaskState Clone()
        {
            return MemberwiseClone() as BackgroundTaskState;
        }
    }
}
