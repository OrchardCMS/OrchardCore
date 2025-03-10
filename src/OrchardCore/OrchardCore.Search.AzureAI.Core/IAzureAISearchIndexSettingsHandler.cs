using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public interface IAzureAISearchIndexSettingsHandler
{
    Task InitializingAsync(AzureAISearchIndexSettingsInitializingContext context);

    Task CreatingAsync(AzureAISearchIndexSettingsCreateContext context);

    Task CreatedAsync(AzureAISearchIndexSettingsCreateContext context);

    Task UpdatingAsync(AzureAISearchIndexSettingsUpdateContext context);

    Task UpdatedAsync(AzureAISearchIndexSettingsUpdateContext context);

    Task ValidatingAsync(AzureAISearchIndexSettingsValidatingContext context);

    Task ResetAsync(AzureAISearchIndexSettingsResetContext context);

    Task SynchronizedAsync(AzureAISearchIndexSettingsSynchronizedContext context);

    Task ExportingAsync(AzureAISearchIndexSettingsExportingContext context);
}
