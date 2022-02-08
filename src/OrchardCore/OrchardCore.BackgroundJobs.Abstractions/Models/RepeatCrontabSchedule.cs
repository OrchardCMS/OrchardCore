using System;

namespace OrchardCore.BackgroundJobs.Models
{
    public class RepeatCrontabSchedule : IBackgroundJobSchedule
    {
        public IBackgroundJobSchedule Initial { get; set; }
        public string RepeatCrontab { get; set; }
        public DateTime ExecutionUtc { get => Initial.ExecutionUtc; set => Initial.ExecutionUtc = value; }
    }
}
