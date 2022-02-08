using System;

namespace OrchardCore.BackgroundJobs.Models
{
    public interface IBackgroundJobSchedule
    {
        DateTime ExecutionUtc { get; set; }
    }
}
