using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs
{
    public interface IBackgroundJobHandlerFactory
    {
        IBackgroundJobHandler Create(string name);
    }
}
