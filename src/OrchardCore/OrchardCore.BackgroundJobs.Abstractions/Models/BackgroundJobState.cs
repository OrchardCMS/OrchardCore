using System;
using System.Collections.Generic;

namespace OrchardCore.BackgroundJobs.Models
{
    public class BackgroundJobState
    {
        public DateTime? CreatedUtc { get; set; }
        public BackgroundJobStatus CurrentStatus { get;  set; }
        public int CurrentRetryCount { get; set;  }

        public void UpdateState(BackgroundJobStatus jobState, DateTime changedUtc, object metadata = null)
        {
            var history = new JobStatusHistory
            {
                Status = jobState,
                ChangedUtc = changedUtc,
                Metadata = metadata
            };

            StatusHistory.Add(history);

            CurrentStatus = jobState;
            if (CreatedUtc is null)
            {
                CreatedUtc = changedUtc;
            }
        }

        public List<JobStatusHistory> StatusHistory { get; set; } = new List<JobStatusHistory>();
    }

    public class JobStatusHistory
    {
        public BackgroundJobStatus Status { get; set; }
        public DateTime ChangedUtc { get; set; }
        public object Metadata { get; set; }
    }

    public enum BackgroundJobStatus
    {
        Scheduled,
        Queued,
        Executing,
        Executed,

        Retrying,
        Failed,
        Cancelled
    }
}
