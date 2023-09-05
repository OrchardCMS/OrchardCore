using System;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskSettings
    {
        /// <summary>
        /// The name of this background task.
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// The display name of this background task.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Whether this background task is enabled or not.
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// The background task schedule as a cron expression.
        /// </summary>
        public string Schedule { get; set; } = "* * * * *";

        /// <summary>
        /// The description of this background task.
        /// </summary>
        public string Description { get; set; } = String.Empty;

        /// <summary>
        /// The timeout in milliseconds to acquire a lock before executing the task atomically.
        /// There is no locking if equal to zero or if there is no registered distributed lock.
        /// </summary>
        public int LockTimeout { get; set; }

        /// <summary>
        /// The expiration in milliseconds of the lock acquired before executing the task atomically.
        /// There is no locking if equal to zero or if there is no registered distributed lock.
        /// </summary>
        public int LockExpiration { get; set; }

        /// <summary>
        /// Whether or not the pipeline should be built to configure endpoints
        /// and then executed to allow route urls generation in the background.
        /// </summary>
        public bool UsePipeline { get; set; }

        /// <summary>
        /// Whether this background task is atomic or not, it is atomic if both
        /// the lock timeout and the lock expiration time are greater than zero.
        /// </summary>
        public bool IsAtomic => LockTimeout > 0 && LockExpiration > 0;
    }
}
