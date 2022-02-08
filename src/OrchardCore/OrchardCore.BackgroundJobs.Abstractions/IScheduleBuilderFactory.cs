using OrchardCore.BackgroundJobs.Schedules;

namespace OrchardCore.BackgroundJobs
{
    public interface IScheduleBuilderFactory
    {
        T Create<T>() where T : IScheduleBuilder;
    }
}
