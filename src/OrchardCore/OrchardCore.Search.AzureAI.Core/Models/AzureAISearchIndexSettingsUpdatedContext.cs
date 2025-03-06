namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexSettingsUpdatedContext
{
    public AzureAISearchIndexSettings Settings { get; }

    public AzureAISearchIndexSettingsUpdatedContext(AzureAISearchIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
}
