using System;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundTasks
{
    public interface IBackgroundTaskStateProvider
    {
        Task<BackgroundTaskState> GetStateAsync(string tenant, string taskName);
    }
}