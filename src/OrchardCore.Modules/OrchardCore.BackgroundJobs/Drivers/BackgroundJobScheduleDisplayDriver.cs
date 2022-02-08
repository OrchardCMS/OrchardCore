using OrchardCore.BackgroundJobs.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.BackgroundJobs.Drivers
{
    public class BackgroundJobScheduleDisplayDriver : DisplayDriver<BackgroundJobExecution>
    {
        public override IDisplayResult Display(BackgroundJobExecution model)
            => View("BackgroundJobSchedule_Fields_SummaryAdmin", model).Location("SummaryAdmin", "Content:5");
    }
}
