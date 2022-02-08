using OrchardCore.BackgroundJobs.Models;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.BackgroundJobs.Drivers
{
    public class RepeatCrontabScheduleDisplayDriver : BackgroundJobScheduleDisplayDriver<RepeatCrontabSchedule>
    {
        public override IDisplayResult Display(BackgroundJobExecution model)
            => View("RepeatCrontabSchedule_Fields_SummaryAdmin", model).Location("SummaryAdmin", "Content:5");

    }
}
