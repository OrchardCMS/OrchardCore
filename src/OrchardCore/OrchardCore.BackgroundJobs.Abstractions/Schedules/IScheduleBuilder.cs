using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs.Schedules
{
    public interface IScheduleBuilder
    {
        ValueTask<IBackgroundJobSchedule> BuildAsync();
    }
}
