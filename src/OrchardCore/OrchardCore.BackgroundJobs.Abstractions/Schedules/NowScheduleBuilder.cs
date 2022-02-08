using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundJobs.Schedules
{
    public class NowScheduleBuilder : IScheduleBuilder
    {
        private readonly IClock _clock;

        public NowScheduleBuilder(IClock clock) => _clock = clock;

        public ValueTask<IBackgroundJobSchedule> BuildAsync()
            => new ValueTask<IBackgroundJobSchedule>(new DateTimeSchedule { ExecutionUtc = _clock.UtcNow });
    }
}
