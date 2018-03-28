using System.Collections.Generic;

namespace OrchardCore.BackgroundTasks.ViewModels
{
    public class BackgroundTaskIndexViewModel
    {
        public bool IsRunning { get; set; }
        public bool HasPendingChanges { get; set; }
        public IList<BackgroundTaskEntry> Tasks { get; set; }
        public dynamic Pager { get; set; }
    }

    public class BackgroundTaskEntry
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public BackgroundTaskSettings Settings { get; set; }
        public BackgroundTaskState State { get; set; }
        public bool HasDocumentSettings { get; set; }
    }
}
