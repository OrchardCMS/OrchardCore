using Microsoft.Extensions.Options;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskAttributeOptionsSetup : IConfigureOptions<BackgroundTaskOptions>
    {
        public void Configure(BackgroundTaskOptions options)
        {
            options.SettingsProviders.Add(new BackgroundTaskAttributeSettingsProvider());
        }
    }
}