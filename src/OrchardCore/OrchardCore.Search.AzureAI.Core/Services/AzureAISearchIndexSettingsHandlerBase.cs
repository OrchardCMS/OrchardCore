using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public abstract class AzureAISearchIndexSettingsHandlerBase : IAzureAISearchIndexSettingsHandler
{
    public virtual Task ExportingAsync(AzureAISearchIndexSettingsExportingContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task InitializingAsync(AzureAISearchIndexSettingsInitializingContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task MappingAsync(AzureAISearchMappingContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task ResetAsync(AzureAISearchIndexSettingsResetContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task SynchronizedAsync(AzureAISearchIndexSettingsSynchronizedContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task UpdatedAsync(AzureAISearchIndexSettingsUpdatedContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task ValidatingAsync(AzureAISearchIndexSettingsValidatingContext context)
    {
        return Task.CompletedTask;
    }
}
