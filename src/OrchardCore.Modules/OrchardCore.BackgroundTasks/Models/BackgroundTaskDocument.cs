using OrchardCore.Data.Documents;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundTasks.Models;

public class BackgroundTaskDocument : Document
{
    private readonly Dictionary<string, BackgroundTaskSettings> _settings = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, BackgroundTaskSettings> Settings
    {
        get => _settings;
        set => _settings.SetItems(value);
    }
}
