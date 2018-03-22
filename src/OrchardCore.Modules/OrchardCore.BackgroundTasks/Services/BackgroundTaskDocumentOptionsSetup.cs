using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace OrchardCore.BackgroundTasks.Services
{
    public class BackgroundTaskDocumentOptionsSetup : IConfigureOptions<BackgroundTaskOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackgroundTaskDocumentOptionsSetup(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Configure(BackgroundTaskOptions options)
        {
            options.SettingsProviders.Add(new BackgroundTaskDocumentSettingsProvider(_httpContextAccessor));
        }
    }
}