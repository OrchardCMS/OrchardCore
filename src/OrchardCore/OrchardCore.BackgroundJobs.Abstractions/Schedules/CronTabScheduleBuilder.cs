using System.Threading.Tasks;
using NCrontab;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundJobs.Schedules
{
    public class CronTabScheduleBuilder : IScheduleBuilder
    {
        private readonly IClock _clock;

        public CronTabScheduleBuilder(IClock clock) => _clock = clock;

        public string Crontab { get; set; }

        public ValueTask<IBackgroundJobSchedule> BuildAsync()
        {
            var schedule = CrontabSchedule.Parse(Crontab);
            var whenUtc = schedule.GetNextOccurrence(_clock.UtcNow);

            return new ValueTask<IBackgroundJobSchedule>(new DateTimeSchedule { ExecutionUtc = whenUtc });
        }
    }
}
