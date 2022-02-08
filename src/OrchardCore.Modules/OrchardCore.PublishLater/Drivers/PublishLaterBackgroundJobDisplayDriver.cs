using OrchardCore.BackgroundJobs.Drivers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.PublishLater.Models;

namespace OrchardCore.PublishLater.Drivers
{
    public class PublishLaterBackgroundJobDisplayDriver : BackgroundJobDisplayDriverBase<PublishLaterBackgroundJob>
    {
        public override IDisplayResult Display(PublishLaterBackgroundJob model)
            => View("PublishLaterBackgroundJob_Fields_SummaryAdmin", model).Location("SummaryAdmin", "Content:1");
    }
}
