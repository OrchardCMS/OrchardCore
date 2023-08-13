using System;

namespace OrchardCore.BackgroundTasks
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class BackgroundTaskAttribute : Attribute
    {
        public string Title { get; set; }
        public bool Enable { get; set; } = true;
        public string Schedule { get; set; } = "*/5 * * * *";
        public string Description { get; set; } = String.Empty;
        public int LockTimeout { get; set; }
        public int LockExpiration { get; set; }

        /// <summary>
        /// Wether or not the tenant pipeline should be built and then executed.
        /// This to configure endpoints and then to allow route urls generation.
        /// </summary>
        public bool UsePipeline { get; set; }
    }
}
