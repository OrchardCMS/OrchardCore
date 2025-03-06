namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexSettingsResetContext
{
    public AzureAISearchIndexSettings Settings { get; }

    public AzureAISearchIndexSettingsResetContext(AzureAISearchIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
}
