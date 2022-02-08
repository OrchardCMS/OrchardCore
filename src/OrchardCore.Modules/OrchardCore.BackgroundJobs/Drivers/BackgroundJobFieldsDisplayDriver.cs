using OrchardCore.BackgroundJobs.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.BackgroundJobs.Drivers
{
    public class BackgroundJobFieldsDisplayDriver : DisplayDriver<BackgroundJobExecution>
    {
        public override IDisplayResult Display(BackgroundJobExecution model)
            => Combine(
                    View("BackgroundJob_Fields_SummaryAdmin", model).Location("SummaryAdmin", "Content:10"),
                    View("BackgroundJob_Buttons_SummaryAdmin", model).Location("SummaryAdmin", "Actions:1")
                );
    }
}
