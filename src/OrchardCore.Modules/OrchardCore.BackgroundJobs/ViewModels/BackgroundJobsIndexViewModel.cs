using System.Collections.Generic;

namespace OrchardCore.BackgroundJobs.ViewModels
{
    public class BackgroundJobsIndexViewModel
    {
        public IList<BackgroundJobViewModelEntry> BackgroundJobs { get; set; }
        public BackgroundJobIndexOptions Options { get; set; } = new BackgroundJobIndexOptions();
        public dynamic Pager { get; set; }
        public dynamic Header { get; set; }
    }

    public class BackgroundJobViewModelEntry
    {
        public dynamic Shape { get; set; }
        public string BackgroundJobId { get; set; }
    }
}
