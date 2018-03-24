using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundTasks
{
    public interface IBackgroundTaskStateProvider
    {
        Task<IEnumerable<BackgroundTaskState>> GetStatesAsync(string tenant);
        Task<BackgroundTaskState> GetStateAsync(string tenant, string taskName);
    }
}