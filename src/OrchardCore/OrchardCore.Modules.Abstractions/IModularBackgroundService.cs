using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Modules
{
    public interface IModularBackgroundService
    {
        Task<BackgroundTaskSettings> GetSettingsAsync(string tenant, string taskName);
        Task<IEnumerable<BackgroundTaskSettings>> GetSettingsAsync(string tenant);
    }
}