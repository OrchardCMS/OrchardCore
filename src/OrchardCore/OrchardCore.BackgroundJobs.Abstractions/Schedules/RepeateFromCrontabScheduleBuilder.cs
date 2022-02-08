using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs.Schedules
{
    public class RepeatFromCrontabScheduleBuilder : IScheduleBuilder
    {
        private readonly CronTabScheduleBuilder _initial;

        public RepeatFromCrontabScheduleBuilder(CronTabScheduleBuilder initial)
        {
            _initial = initial;
        }

        public async ValueTask<IBackgroundJobSchedule> BuildAsync()
        {
            var initial = await _initial.BuildAsync();

            return new RepeatCrontabSchedule { Initial = initial, RepeatCrontab = _initial.Crontab };
        }
    }
}
