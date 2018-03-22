using System;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundTasks.Services
{
    public class BackgroundTaskDefinitionProvider : IBackgroundTaskDefinitionProvider
    {
        private readonly BackgroundTaskManager _backgroundTaskManager;

        public BackgroundTaskDefinitionProvider(BackgroundTaskManager backgroundTaskManager)
        {
            _backgroundTaskManager = backgroundTaskManager;
        }

        public int Order => 50;

        public async Task<BackgroundTaskDefinition> GetDefinitionAsync(Type type)
        {
            var document = await _backgroundTaskManager.GetDocumentAsync();

            if (document.Tasks.TryGetValue(type.FullName, out var task))
            {
                return new BackgroundTaskDefinition()
                {
                    Enable = task.Enable,
                    Schedule = task.Schedule,
                    Description = task.Description
                };
            }

            return new NotFoundBackgroundTaskDefinition();
        }
    }
}