using System.Text.Json.Serialization;
using OrchardCore.Data.Documents;
using OrchardCore.Json.Serialization;

namespace OrchardCore.BackgroundTasks.Models;

public class BackgroundTaskDocument : Document
{
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<BackgroundTaskSettings>))]
    public Dictionary<string, BackgroundTaskSettings> Settings { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
