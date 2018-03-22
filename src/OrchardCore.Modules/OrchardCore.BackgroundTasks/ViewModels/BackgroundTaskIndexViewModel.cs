using System.Collections.Generic;
using OrchardCore.BackgroundTasks.Models;

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
        public BackgroundTaskDefinition Definition { get; set; }
        public bool IsChecked { get; set; }
    }
}
