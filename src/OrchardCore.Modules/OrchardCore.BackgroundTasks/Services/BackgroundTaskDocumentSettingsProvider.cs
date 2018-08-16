using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.BackgroundTasks.Services
{
    public class BackgroundTaskDocumentSettingsProvider : IBackgroundTaskSettingsProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackgroundTaskDocumentSettingsProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int Order => 50;

        public async Task<BackgroundTaskSettings> GetSettingsAsync(Type type)
        {
            var backgroundTaskManager = _httpContextAccessor.HttpContext
                .RequestServices.GetRequiredService<BackgroundTaskManager>();

            var document = await backgroundTaskManager.GetDocumentAsync();

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