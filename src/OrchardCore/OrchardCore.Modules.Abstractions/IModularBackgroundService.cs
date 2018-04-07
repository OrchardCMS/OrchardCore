using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Modules
{
    public interface IModularBackgroundService
    {
        bool IsRunning { get; }
        void Command(string tenant, string taskName, BackgroundTaskScheduler.CommandCode code);
        Task<BackgroundTaskSettings> GetSettingsAsync(string tenant, string taskName);
        Task<IEnumerable<BackgroundTaskSettings>> GetSettingsAsync(string tenant);
        Task<BackgroundTaskState> GetStateAsync(string tenant, string taskName);
        Task<IEnumerable<BackgroundTaskState>> GetStatesAsync(string tenant);
        Task UpdateAsync(string tenant, string taskName);
        Task UpdateAsync(string tenant);
    }
}