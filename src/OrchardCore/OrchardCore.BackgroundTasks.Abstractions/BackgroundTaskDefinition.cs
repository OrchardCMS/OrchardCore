using System;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskDefinition
    {
        public bool Enable { get; set; } = true;
        public string Schedule { get; set; } = "* * * * *";
        public string Description { get; set; } = String.Empty;
    }

    public class NotFoundBackgroundTaskDefinition : BackgroundTaskDefinition
    {
    }
}