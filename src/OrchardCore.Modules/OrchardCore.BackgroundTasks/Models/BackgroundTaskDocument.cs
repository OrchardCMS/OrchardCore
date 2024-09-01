using OrchardCore.Data.Documents;

namespace OrchardCore.BackgroundTasks.Models;

public class BackgroundTaskDocument : Document
{
    public Dictionary<string, BackgroundTaskSettings> Settings { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
