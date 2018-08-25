using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.BackgroundTasks
{
    public interface IBackgroundTaskSettingsProvider
    {
        int Order { get; }
        IChangeToken ChangeToken { get;  }
        Task<BackgroundTaskSettings> GetSettingsAsync(Type type);
    }
}