using System;

namespace OrchardCore.BackgroundJobs.Models
{
    public class BackgroundJobEntry : IBackgroundJobSchedule
    {
        public BackgroundJobEntry(string backgroundJobId, string name, BackgroundJobStatus status, DateTime executionUtc)
        {
            BackgroundJobId = backgroundJobId;
            Name = name;
            Status = status;
            ExecutionUtc = executionUtc;
        }

        public string BackgroundJobId { get; }
        public string Name { get; }
        public BackgroundJobStatus Status { get; }
        public DateTime ExecutionUtc { get; set; }
    }
}
