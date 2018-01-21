using OrchardCore.Workflows.Display;

namespace OrchardCore.Workflows.Timers
{
    public class TimerEventDisplay : ActivityDisplayDriver<TimerEvent, TimerEventViewModel>
    {
        protected override void Map(TimerEvent source, TimerEventViewModel target)
        {
            target.CronExpression = source.CronExpression;
        }

        protected override void Map(TimerEventViewModel source, TimerEvent target)
        {
            target.CronExpression = source.CronExpression.Trim();
        }
    }
}
