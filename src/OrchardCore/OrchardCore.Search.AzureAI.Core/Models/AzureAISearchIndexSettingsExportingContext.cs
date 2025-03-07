using System.Text.Json.Nodes;

namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexSettingsExportingContext
{
    public JsonNode Data { get; }

    public AzureAISearchIndexSettings Settings { get; }

    public AzureAISearchIndexSettingsExportingContext(AzureAISearchIndexSettings settings, JsonNode data)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
        Data = data ?? new JsonObject();
    }
}
