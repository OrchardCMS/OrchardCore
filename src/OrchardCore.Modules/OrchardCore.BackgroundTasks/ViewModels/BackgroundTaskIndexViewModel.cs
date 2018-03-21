using System.Collections.Generic;
using OrchardCore.BackgroundTasks.Models;

namespace OrchardCore.BackgroundTasks.ViewModels
{
    public class BackgroundTaskIndexViewModel
    {
        public IList<BackgroundTaskEntry> BackgroundTasks { get; set; }
        public dynamic Pager { get; set; }
    }

    public class BackgroundTaskEntry
    {
        public string Name { get; set; }
        public BackgroundTask BackgroundTask { get; set; }
        public bool IsChecked { get; set; }
    }
}
