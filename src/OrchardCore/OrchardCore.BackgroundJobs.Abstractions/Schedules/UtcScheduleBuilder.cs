using System;
using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs.Schedules
{
    public class UtcScheduleBuilder : IScheduleBuilder
    {
        public DateTime Utc { get; set; }

        public ValueTask<IBackgroundJobSchedule> BuildAsync()
            => new ValueTask<IBackgroundJobSchedule>(new DateTimeSchedule { ExecutionUtc = Utc });
    }
}
