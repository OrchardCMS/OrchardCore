namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexSettingsUpdateContext
{
    public AzureAISearchIndexSettings Settings { get; }

    public AzureAISearchIndexSettingsUpdateContext(AzureAISearchIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
}
