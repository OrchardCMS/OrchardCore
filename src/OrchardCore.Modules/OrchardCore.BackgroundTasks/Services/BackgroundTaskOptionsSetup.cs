using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace OrchardCore.BackgroundTasks.Services
{
    public class BackgroundTaskOptionsSetup : IConfigureOptions<BackgroundTasksOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackgroundTaskOptionsSetup(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        public void Configure(BackgroundTasksOptions options)
        {
            options.OptionsProviders.Add(new BackgroundTaskOptionsProvider(_httpContextAccessor));
        }
    }
}