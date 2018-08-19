using System;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundTasks.Services
{
    public class BackgroundTaskDocumentSettingsProvider : IBackgroundTaskSettingsProvider
    {
        private readonly BackgroundTaskManager _backgroundTaskManager;

        public BackgroundTaskDocumentSettingsProvider(BackgroundTaskManager backgroundTaskManager)
        {
            _backgroundTaskManager = backgroundTaskManager;
        }

        public int Order => 50;

        public async Task<BackgroundTaskSettings> GetSettingsAsync(Type type)
        {
            var document = await _backgroundTaskManager.GetDocumentAsync();

            if (document.Tasks.TryGetValue(type.FullName, out var settings))
            {
                return new BackgroundTaskSettings()
                {
                    Name = type.FullName,
                    Enable = settings.Enable,
                    Schedule = settings.Schedule,
                    Description = settings.Description
                };
            }

            return BackgroundTaskSettings.None;
        }
    }
}