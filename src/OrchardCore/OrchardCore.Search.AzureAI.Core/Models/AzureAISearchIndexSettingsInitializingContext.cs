using System.Text.Json.Nodes;

namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexSettingsInitializingContext
{
    public AzureAISearchIndexSettings Settings { get; }

    public JsonNode Data { get; }

    public AzureAISearchIndexSettingsInitializingContext(AzureAISearchIndexSettings settings, JsonNode data)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
        Data = data ?? new JsonObject();
    }
}
