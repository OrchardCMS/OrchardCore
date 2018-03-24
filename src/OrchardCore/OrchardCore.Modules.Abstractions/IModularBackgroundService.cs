using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Modules
{
    public interface IModularBackgroundService
    {
        Task<IEnumerable<BackgroundTaskSettings>> GetSettingsAsync(string tenant);
        Task<IEnumerable<BackgroundTaskState>> GetStatesAsync(string tenant);
        Task<BackgroundTaskSettings> GetSettingsAsync(string tenant, string taskName);
        Task<BackgroundTaskState> GetStateAsync(string tenant, string taskName);
    }
}