using System.Collections.Generic;
using OrchardCore.BackgroundJobs.Services;

namespace OrchardCore.BackgroundJobs.ViewModels
{
    public class BackgroundJobOptionIndexViewModel
    {
        public IList<BackgroundJobOptionEntry> JobOptions { get; set; }
        public BackgroundJobTypeIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class BackgroundJobOptionEntry
    {
        public BackgroundJobOption JobOption { get; set; }
        public bool IsChecked { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int WorkflowCount { get; set; }
    }

    public class BackgroundJobTypeIndexOptions
    {
        public string Search { get; set; }
        public BackgroundJobTypeOrder Order { get; set; }
        public BackgroundJobTypeFilter Filter { get; set; }
    }

    public enum BackgroundJobTypeOrder
    {
        Name,
        // TODO failed.
    }

    public enum BackgroundJobTypeFilter
    {
        All
    }
}
