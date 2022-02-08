using System;

namespace OrchardCore.BackgroundJobs.Models
{
    public class DateTimeSchedule : IBackgroundJobSchedule
    {
        public DateTime ExecutionUtc { get; set; }
    }
}
