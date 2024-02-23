using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.BackgroundTasks.ViewModels
{
    public class BackgroundTaskViewModel
    {
        public string Name { get; set; }

        public string Title { get; set; }

        [BindNever]
        public string DefaultSchedule { get; set; }

        public string Schedule { get; set; }

        public string Description { get; set; }

        public bool Enable { get; set; } = true;

        public int LockTimeout { get; set; }

        public int LockExpiration { get; set; }

        public bool UsePipeline { get; set; }

        public bool IsAtomic => LockTimeout > 0 && LockExpiration > 0;
    }
}
