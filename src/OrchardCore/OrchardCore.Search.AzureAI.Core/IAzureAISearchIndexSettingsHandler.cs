using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public interface IAzureAISearchIndexSettingsHandler
{
    Task InitializingAsync(AzureAISearchIndexSettingsInitializingContext context);

    Task UpdatedAsync(AzureAISearchIndexSettingsUpdatedContext context);

    Task ValidatingAsync(AzureAISearchIndexSettingsValidatingContext context);

    Task MappingAsync(AzureAISearchMappingContext context);

    Task ResetAsync(AzureAISearchIndexSettingsResetContext context);

    Task SynchronizedAsync(AzureAISearchIndexSettingsSynchronizedContext context);

    Task ExportingAsync(AzureAISearchIndexSettingsExportingContext context);
}
