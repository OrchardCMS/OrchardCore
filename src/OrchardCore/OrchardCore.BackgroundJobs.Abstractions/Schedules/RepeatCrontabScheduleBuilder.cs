using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs.Schedules
{
    public class RepeatCrontabScheduleBuilder : IScheduleBuilder
    {
        private readonly IScheduleBuilder _initial;
        public readonly string _crontab;

        public RepeatCrontabScheduleBuilder(IScheduleBuilder initial, string crontab)
        {
            _initial = initial;
            _crontab = crontab;
        }

        public async ValueTask<IBackgroundJobSchedule> BuildAsync()
        {
            var initial = await _initial.BuildAsync();

            return new RepeatCrontabSchedule { Initial = initial, RepeatCrontab = _crontab };
        }
    }
}
