using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.BackgroundTasks.ViewModels
{
    public class BackgroundTaskIndexViewModel
    {
        public AdminIndexOptions Options { get; set; }

        [BindNever]
        public IList<BackgroundTaskEntry> Tasks { get; set; }

        [BindNever]
        public dynamic Pager { get; set; }
    }

    public class BackgroundTaskEntry
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public bool Enable { get; set; }

        public string Description { get; set; }
    }
}
