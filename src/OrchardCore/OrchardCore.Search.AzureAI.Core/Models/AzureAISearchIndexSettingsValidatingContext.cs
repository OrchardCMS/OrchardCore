namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexSettingsValidatingContext
{
    public ValidationResultDetails Result { get; } = new();

    public AzureAISearchIndexSettings Settings { get; }

    public AzureAISearchIndexSettingsValidatingContext(AzureAISearchIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
}
