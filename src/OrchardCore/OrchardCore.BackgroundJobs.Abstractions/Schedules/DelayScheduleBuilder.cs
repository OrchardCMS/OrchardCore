using System;
using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundJobs.Schedules
{
    public class DelayScheduleBuilder : IScheduleBuilder
    {
        private readonly IClock _clock;

        public DelayScheduleBuilder(IClock clock) => _clock = clock;

        public TimeSpan Delay { get; set; } = TimeSpan.Zero;

        public ValueTask<IBackgroundJobSchedule> BuildAsync()
            => new ValueTask<IBackgroundJobSchedule>(new DateTimeSchedule { ExecutionUtc = _clock.UtcNow.Add(Delay) });
    }
}
