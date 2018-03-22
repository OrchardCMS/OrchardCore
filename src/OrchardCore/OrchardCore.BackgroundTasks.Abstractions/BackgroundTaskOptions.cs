using System.Collections.Generic;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTasksOptions
    {
        public IList<IBackgroundTaskOptionsProvider> OptionsProviders = new List<IBackgroundTaskOptionsProvider>();
    }

    public class BackgroundTaskOptions
    {
        public bool Enable { get; set; } = true;
        public string Schedule { get; set; } = "* * * * *";
    }

    public class NotFoundBackgroundTaskOptions : BackgroundTaskOptions
    {
    }
}