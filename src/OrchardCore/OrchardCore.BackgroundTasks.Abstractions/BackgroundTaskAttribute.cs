using System;

namespace OrchardCore.BackgroundTasks
{
    /// <summary>
    /// When applied to a class implementing <see cref="IBackgroundTask"/>, defines the group
    /// the task belongs to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class BackgroundTaskAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the group this tasks belongs to. Tasks which are part of the same group are
        /// run sequentially within the group.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// The minimum period of the parent group. All tasks which are part of the same group may
        /// provide a value but only the maximun is retained, the default value being of 1 minute.
        /// </summary>
        public int Minutes { get; set; }
    }
}
