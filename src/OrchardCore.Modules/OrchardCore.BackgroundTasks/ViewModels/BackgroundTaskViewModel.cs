using System.Collections.Generic;

namespace OrchardCore.BackgroundTasks.ViewModels
{
    public class BackgroundTaskViewModel
    {
        public string Name { get; set; }
        public bool Enable { get; set; }
        public string Schedule { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Names { get; set; }
    }
}
