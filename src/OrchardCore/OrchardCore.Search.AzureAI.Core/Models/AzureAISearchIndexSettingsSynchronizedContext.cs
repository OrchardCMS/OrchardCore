namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexSettingsSynchronizedContext
{
    public AzureAISearchIndexSettings Settings { get; }

    public AzureAISearchIndexSettingsSynchronizedContext(AzureAISearchIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
}
