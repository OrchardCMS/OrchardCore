using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAISearchIndexSettingsService
{
    private readonly IEnumerable<IAzureAISearchIndexSettingsHandler> _handlers;
    private readonly ILogger _logger;

    public AzureAISearchIndexSettingsService(
        IEnumerable<IAzureAISearchIndexSettingsHandler> handlers,
        ILogger<AzureAISearchIndexSettingsService> logger)
    {
        _handlers = handlers;
        _logger = logger;
    }

    /// <summary>
    /// Loads the index settings document from the store for updating and that should not be cached.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static
    public Task<AzureAISearchIndexSettingsDocument> LoadDocumentAsync()
#pragma warning restore CA1822 // Mark members as static
        => DocumentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the index settings document from the cache for sharing and that should not be updated.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static
    public Task<AzureAISearchIndexSettingsDocument> GetDocumentAsync()
#pragma warning restore CA1822 // Mark members as static
    {
        return DocumentManager.GetOrCreateImmutableAsync();
    }

    public async Task<IEnumerable<AzureAISearchIndexSettings>> GetSettingsAsync()
        => (await GetDocumentAsync()).IndexSettings.Values;

    public async Task<AzureAISearchIndexSettings> GetAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        var document = await GetDocumentAsync();

        if (document.IndexSettings.TryGetValue(id, out var settings))
        {
            return settings;
        }

        return null;
    }

    public async Task UpdateAsync(AzureAISearchIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var document = await LoadDocumentAsync();

        if (document.IndexSettings.Values.Any(x => x.IndexName == settings.IndexName && x.Source == settings.Source && x.Id != settings.Id))
        {
            throw new InvalidOperationException("Another index with the same name already exists.");
        }

        document.IndexSettings[settings.Id] = settings;
        await DocumentManager.UpdateAsync(document);

        var updatedContext = new AzureAISearchIndexSettingsUpdatedContext(settings);
        await _handlers.InvokeAsync((handler, ctx) => handler.UpdatedAsync(ctx), updatedContext, _logger);
    }

    public async Task SynchronizeAsync(AzureAISearchIndexSettings settings)
    {
        var synchronizedContext = new AzureAISearchIndexSettingsSynchronizedContext(settings);
        await _handlers.InvokeAsync((handler, ctx) => handler.SynchronizedAsync(ctx), synchronizedContext, _logger);
    }

    public async Task DeleteAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        var document = await LoadDocumentAsync();

        document.IndexSettings.Remove(id);

        await DocumentManager.UpdateAsync(document);
    }

    public async Task<AzureAISearchIndexSettings> NewAsync(string source, JsonNode data = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);

        var id = IdGenerator.GenerateId();

        var model = new AzureAISearchIndexSettings()
        {
            Id = id,
            Source = source,
        };

        var initializingContext = new AzureAISearchIndexSettingsInitializingContext(model, data);
        await _handlers.InvokeAsync((handler, ctx) => handler.InitializingAsync(ctx), initializingContext, _logger);

        // Set the source again after calling handlers to prevent handlers from updating the source during initialization.
        model.Source = source;

        if (string.IsNullOrEmpty(model.Id))
        {
            model.Id = id;
        }

        return model;
    }

    public async Task<ValidationResultDetails> ValidateAsync(AzureAISearchIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var validatingContext = new AzureAISearchIndexSettingsValidatingContext(settings);
        await _handlers.InvokeAsync((handler, ctx) => handler.ValidatingAsync(ctx), validatingContext, _logger);

        return validatingContext.Result;
    }

    public async Task ResetAsync(AzureAISearchIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var validatingContext = new AzureAISearchIndexSettingsResetContext(settings);
        await _handlers.InvokeAsync((handler, ctx) => handler.ResetAsync(ctx), validatingContext, _logger);
    }

    private static IDocumentManager<AzureAISearchIndexSettingsDocument> DocumentManager
        => ShellScope.Services.GetRequiredService<IDocumentManager<AzureAISearchIndexSettingsDocument>>();
}
