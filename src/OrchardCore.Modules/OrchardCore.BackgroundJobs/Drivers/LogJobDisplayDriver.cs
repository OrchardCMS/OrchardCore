using OrchardCore.BackgroundJobs.Models;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.BackgroundJobs.Drivers
{
    public class LogJobDisplayDriver : BackgroundJobDisplayDriverBase<LogJob2>
    {
        public override IDisplayResult Display(LogJob2 model)
            => View("LogJob_Fields_SummaryAdmin", model).Location("SummaryAdmin", "Content:1");
    }
}
