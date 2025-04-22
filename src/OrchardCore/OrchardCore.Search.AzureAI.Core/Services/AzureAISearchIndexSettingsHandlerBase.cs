using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public abstract class AzureAISearchIndexSettingsHandlerBase : IAzureAISearchIndexSettingsHandler
{
    public virtual Task CreatedAsync(AzureAISearchIndexSettingsCreateContext context)
        => Task.CompletedTask;

    public virtual Task CreatingAsync(AzureAISearchIndexSettingsCreateContext context)
        => Task.CompletedTask;

    public virtual Task ExportingAsync(AzureAISearchIndexSettingsExportingContext context)
        => Task.CompletedTask;

    public virtual Task InitializingAsync(AzureAISearchIndexSettingsInitializingContext context)
        => Task.CompletedTask;

    public virtual Task ResetAsync(AzureAISearchIndexSettingsResetContext context)
        => Task.CompletedTask;

    public virtual Task SynchronizedAsync(AzureAISearchIndexSettingsSynchronizedContext context)
        => Task.CompletedTask;

    public virtual Task UpdatingAsync(AzureAISearchIndexSettingsUpdateContext context)
        => Task.CompletedTask;

    public virtual Task UpdatedAsync(AzureAISearchIndexSettingsUpdateContext context)
        => Task.CompletedTask;

    public virtual Task ValidatingAsync(AzureAISearchIndexSettingsValidatingContext context)
        => Task.CompletedTask;
}
