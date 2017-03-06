using System;

namespace Orchard.BackgroundTasks
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
    }
}
