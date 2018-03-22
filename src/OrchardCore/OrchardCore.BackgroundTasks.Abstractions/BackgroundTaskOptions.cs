using System.Collections.Generic;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskOptions
    {
        public IList<IBackgroundTaskSettingsProvider> SettingsProviders = new List<IBackgroundTaskSettingsProvider>();
    }
}