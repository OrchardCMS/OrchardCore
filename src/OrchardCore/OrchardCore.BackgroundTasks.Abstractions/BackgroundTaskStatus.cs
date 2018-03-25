namespace OrchardCore.BackgroundTasks
{
    public enum BackgroundTaskStatus
    {
        /// <summary>
        /// The task is idle, i.e. awaiting for the next scheduled execution.
        /// </summary>
        Idle,
        /// <summary>
        /// The task is currently running.
        /// </summary>
        Running,
        /// <summary>
        /// The task is currently locked.
        /// </summary>
        Locked,
    }
}
