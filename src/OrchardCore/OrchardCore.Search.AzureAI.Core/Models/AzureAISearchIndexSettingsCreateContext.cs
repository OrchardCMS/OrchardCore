namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexSettingsCreateContext
{
    public AzureAISearchIndexSettings Settings { get; }

    public AzureAISearchIndexSettingsCreateContext(AzureAISearchIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
}
