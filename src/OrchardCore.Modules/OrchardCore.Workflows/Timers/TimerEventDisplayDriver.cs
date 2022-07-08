using OrchardCore.Workflows.Display;

namespace OrchardCore.Workflows.Timers
{
    public class TimerEventDisplayDriver : ActivityDisplayDriver<TimerEvent, TimerEventViewModel>
    {
        protected override void EditActivity(TimerEvent source, TimerEventViewModel model)
        {
            model.CronExpression = source.CronExpression;
        }

        protected override void UpdateActivity(TimerEventViewModel model, TimerEvent target)
        {
            target.CronExpression = model.CronExpression.Trim();
        }
    }
}
