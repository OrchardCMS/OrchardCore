using System;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundTasks
{
    public interface IBackgroundTaskSettingsProvider
    {
        int Order { get; }
        Task<BackgroundTaskSettings> GetSettingsAsync(Type type);
    }
}