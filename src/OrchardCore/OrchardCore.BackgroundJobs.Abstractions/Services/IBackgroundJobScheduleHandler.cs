using System.Linq;
using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs.Services
{
    public interface IBackgroundJobScheduleHandler
    {
        (bool CanRun, long Priority) CanRun(IBackgroundJobSchedule schedule);
    }
}
