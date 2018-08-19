using System.Collections.Generic;

namespace OrchardCore.BackgroundTasks.ViewModels
{
    public class BackgroundTaskIndexViewModel
    {
        public IList<BackgroundTaskEntry> Tasks { get; set; }
        public dynamic Pager { get; set; }
    }

    public class BackgroundTaskEntry
    {
        public string Name { get; set; }
        public BackgroundTaskSettings Settings { get; set; }
    }
}
