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
        /// Wether or not the shell pipeline needs to be built if not yet done.
        /// This to configure endpoints and then allow to generate routed urls.
        /// </summary>
        public bool PipelineWarmup { get; set; }
    }
}
