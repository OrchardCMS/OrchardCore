using Microsoft.Extensions.Options;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskOptionsSetup : IConfigureOptions<BackgroundTasksOptions>
    {
        public void Configure(BackgroundTasksOptions options)
        {
            options.OptionsProviders.Add(new BackgroundTaskOptionsProvider());
        }
    }
}