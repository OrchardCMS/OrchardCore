using System;

namespace OrchardCore.BackgroundTasks
{
    /// <summary>
    /// When applied to a class implementing <see cref="IBackgroundTask"/>, defines the
    /// schedule of this task as a cron expression.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class BackgroundTaskAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the schedule of this task as a cron expression.
        /// If not provided, defaults to every minutes.
        /// </summary>
        public string Schedule { get; set; }
    }
}
