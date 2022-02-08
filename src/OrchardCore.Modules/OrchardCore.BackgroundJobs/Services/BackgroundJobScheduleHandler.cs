using OrchardCore.BackgroundJobs.Models;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundJobs.Services
{
    public class BackgroundJobScheduleHandler : IBackgroundJobScheduleHandler
    {
        private readonly IClock _clock;

        public BackgroundJobScheduleHandler(IClock clock)
        {
            _clock = clock;
        }

        public (bool CanRun, long Priority) CanRun(IBackgroundJobSchedule schedule)
        {
            if (schedule.ExecutionUtc < _clock.UtcNow)
            {
                return (true, (_clock.UtcNow - schedule.ExecutionUtc).Ticks);
            }

            return (false, 0);
        }
    }
}
