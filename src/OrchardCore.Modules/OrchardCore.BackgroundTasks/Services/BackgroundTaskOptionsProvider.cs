using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.BackgroundTasks.Services
{
    public class BackgroundTaskOptionsProvider : IBackgroundTaskOptionsProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackgroundTaskOptionsProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int Order => 50;

        public async Task<BackgroundTaskOptions> GetOptionsAsync(Type type)
        {
            var backgroundTaskManager = _httpContextAccessor.HttpContext
                .RequestServices.GetRequiredService<BackgroundTaskManager>();

            var document = await backgroundTaskManager.GetDocumentAsync();

            if (document.Tasks.TryGetValue(type.FullName, out var task))
            {
                return new BackgroundTaskOptions()
                {
                    Enable = task.Enable,
                    Schedule = task.Schedule,
                };
            }

            return new NotFoundBackgroundTaskOptions();
        }
    }
}