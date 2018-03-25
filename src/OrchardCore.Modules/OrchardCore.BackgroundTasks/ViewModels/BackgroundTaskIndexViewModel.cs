using System.Collections.Generic;
using OrchardCore.BackgroundTasks.Models;

namespace OrchardCore.BackgroundTasks.ViewModels
{
    public class BackgroundTaskIndexViewModel
    {
        public bool IsRunning { get; set; }
        public IList<BackgroundTaskEntry> Tasks { get; set; }
        public dynamic Pager { get; set; }
    }

    public class BackgroundTaskEntry
    {
        public string Name { get; set; }
        public BackgroundTaskDefinition Definition { get; set; }
        public BackgroundTaskSettings Settings { get; set; }
        public BackgroundTaskState State { get; set; }
        public bool IsChecked { get; set; }
    }
}
