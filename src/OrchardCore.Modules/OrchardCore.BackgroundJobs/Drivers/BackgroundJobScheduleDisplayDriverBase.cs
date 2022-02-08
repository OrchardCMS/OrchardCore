using OrchardCore.BackgroundJobs.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.BackgroundJobs.Drivers
{
    public abstract class BackgroundJobScheduleDisplayDriver<TSchedule> : DisplayDriver<BackgroundJobExecution> where TSchedule : IBackgroundJobSchedule
    {
        public override bool CanHandleModel(BackgroundJobExecution model)
        {
            if (model.Schedule is TSchedule)
            {
                return true;

            }

            return false;
        }
    }
}
