using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Models;
using OrchardCore.Search.Elasticsearch.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public class ElasticsearchIndexSettingsService
{
    private readonly IEnumerable<IElasticsearchIndexSettingsHandler> _handlers;
    private readonly ILogger _logger;

    public ElasticsearchIndexSettingsService(
        IEnumerable<IElasticsearchIndexSettingsHandler> handlers,
        ILogger<ElasticsearchIndexSettingsService> logger)
    {
        _handlers = handlers;
        _logger = logger;
    }

    /// <summary>
    /// Loads the index settings document from the store for updating and that should not be cached.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static
    public Task<ElasticIndexSettingsDocument> LoadDocumentAsync()
        => DocumentManager.GetOrCreateMutableAsync();
#pragma warning restore CA1822 // Mark members as static

    /// <summary>
    /// Gets the index settings document from the cache for sharing and that should not be updated.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static
    public Task<ElasticIndexSettingsDocument> GetDocumentAsync()
#pragma warning restore CA1822 // Mark members as static
     => DocumentManager.GetOrCreateImmutableAsync();

    public async Task<IEnumerable<ElasticIndexSettings>> GetSettingsAsync()
        => (await GetDocumentAsync()).ElasticIndexSettings.Values;

    public async Task<ElasticIndexSettings> FindByIdAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        var document = await GetDocumentAsync();

        if (document.ElasticIndexSettings.TryGetValue(id, out var settings))
        {
            return settings;
        }

        return null;
    }

    public async Task<ElasticIndexSettings> FindByNameAsync(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        var document = await GetDocumentAsync();

        return document.ElasticIndexSettings.Values.FirstOrDefault(x => x.IndexName == indexName);
    }

    [Obsolete("Use FindByIdAsync or FindByNameAsync")]
    public Task<ElasticIndexSettings> GetSettingsAsync(string indexName)
        => FindByNameAsync(indexName);

    /// <summary>
    /// Returns the name of he index-time analyzer.
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    public async Task<string> GetIndexAnalyzerAsync(string indexName)
    {
        var document = await GetDocumentAsync();

        return GetAnalyzerName(document, indexName);
    }

    /// <summary>
    /// Returns the name of the query-time analyzer.
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    public async Task<string> GetQueryAnalyzerAsync(string indexName)
    {
        var document = await GetDocumentAsync();

        if (document.ElasticIndexSettings.TryGetValue(indexName, out var settings) && !string.IsNullOrEmpty(settings.QueryAnalyzerName))
        {
            return settings.QueryAnalyzerName;
        }

        return ElasticsearchConstants.DefaultAnalyzer;
    }

    public async Task<string> LoadIndexAnalyzerAsync(string indexName)
    {
        var document = await LoadDocumentAsync();

        return GetAnalyzerName(document, indexName);
    }

    public async Task UpdateIndexAsync(ElasticIndexSettings settings)
    {
        var document = await LoadDocumentAsync();

        if (document.ElasticIndexSettings.Values.Any(x => x.IndexName == settings.IndexName && x.Id != settings.Id))
        {
            throw new InvalidOperationException("Another index with the same name already exists.");
        }

        var updatedContext = new ElasticsearchIndexSettingsUpdateContext(settings);
        await _handlers.InvokeAsync((handler, ctx) => handler.UpdatingAsync(ctx), updatedContext, _logger);

        if (settings.IndexMappings.Count == 0)
        {
            throw new InvalidOperationException("At least one index-mapping is required.");
        }

        document.ElasticIndexSettings[settings.Id] = settings;
        await DocumentManager.UpdateAsync(document);

        await _handlers.InvokeAsync((handler, ctx) => handler.UpdatedAsync(ctx), updatedContext, _logger);
    }


    public async Task<ElasticIndexSettings> NewAsync(string source, JsonNode data = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);

        var id = IdGenerator.GenerateId();

        var model = new ElasticIndexSettings()
        {
            Id = id,
            Source = source,
        };

        var initializingContext = new ElasticsearchIndexSettingsInitializingContext(model, data);
        await _handlers.InvokeAsync((handler, ctx) => handler.InitializingAsync(ctx), initializingContext, _logger);

        // Set the source again after calling handlers to prevent handlers from updating the source during initialization.
        model.Source = source;

        if (string.IsNullOrEmpty(model.Id))
        {
            model.Id = id;
        }

        return model;
    }

    public async Task CreateAsync(ElasticIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var document = await LoadDocumentAsync();

        if (document.ElasticIndexSettings.Values.Any(x => x.IndexName == settings.IndexName && x.Id != settings.Id))
        {
            throw new InvalidOperationException("Another index with the same name already exists.");
        }

        var updatedContext = new ElasticsearchIndexSettingsCreateContext(settings);
        await _handlers.InvokeAsync((handler, ctx) => handler.CreatingAsync(ctx), updatedContext, _logger);

        if (settings.IndexMappings.Count == 0)
        {
            throw new InvalidOperationException("At least one index-mapping is required.");
        }

        document.ElasticIndexSettings[settings.Id] = settings;
        await DocumentManager.UpdateAsync(document);

        await _handlers.InvokeAsync((handler, ctx) => handler.CreatedAsync(ctx), updatedContext, _logger);
    }

    public async Task UpdateAsync(ElasticIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var document = await LoadDocumentAsync();

        if (document.ElasticIndexSettings.Values.Any(x => x.IndexName == settings.IndexName && x.Id != settings.Id))
        {
            throw new InvalidOperationException("Another index with the same name already exists.");
        }

        var updatedContext = new ElasticsearchIndexSettingsUpdateContext(settings);
        await _handlers.InvokeAsync((handler, ctx) => handler.UpdatingAsync(ctx), updatedContext, _logger);

        if (settings.IndexMappings.Count == 0)
        {
            throw new InvalidOperationException("At least one index-mapping is required.");
        }

        document.ElasticIndexSettings[settings.Id] = settings;
        await DocumentManager.UpdateAsync(document);

        await _handlers.InvokeAsync((handler, ctx) => handler.UpdatedAsync(ctx), updatedContext, _logger);
    }

    public async Task SynchronizeAsync(ElasticIndexSettings settings)
    {
        var synchronizedContext = new ElasticsearchIndexSettingsSynchronizedContext(settings);
        await _handlers.InvokeAsync((handler, ctx) => handler.SynchronizedAsync(ctx), synchronizedContext, _logger);
    }

    public async Task<ValidationResultDetails> ValidateAsync(ElasticIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var validatingContext = new ElasticsearchIndexSettingsValidatingContext(settings);
        await _handlers.InvokeAsync((handler, ctx) => handler.ValidatingAsync(ctx), validatingContext, _logger);

        return validatingContext.Result;
    }

    public async Task ResetAsync(ElasticIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var validatingContext = new ElasticsearchIndexSettingsResetContext(settings);
        await _handlers.InvokeAsync((handler, ctx) => handler.ResetAsync(ctx), validatingContext, _logger);
    }

    public async Task<bool> DeleteByIdAsync(string id)
    {
        var document = await LoadDocumentAsync();

        if (document.ElasticIndexSettings.Remove(id))
        {
            await DocumentManager.UpdateAsync(document);

            return true;
        }

        return false;
    }

    public async Task<bool> DeleteByNameAsync(string indexName)
    {
        var document = await LoadDocumentAsync();
        var index = document.ElasticIndexSettings.Values.FirstOrDefault(x => x.IndexName == indexName);

        if (index is not null)
        {
            await DocumentManager.UpdateAsync(document);

            return true;
        }

        return false;
    }

    public async Task DeleteIndexAsync(string indexName)
    {
        var document = await LoadDocumentAsync();
        document.ElasticIndexSettings.Remove(indexName);
        await DocumentManager.UpdateAsync(document);
    }

    private static IDocumentManager<ElasticIndexSettingsDocument> DocumentManager =>
        ShellScope.Services.GetRequiredService<IDocumentManager<ElasticIndexSettingsDocument>>();

    // Returns the name of the analyzer configured for the given index name.
    private static string GetAnalyzerName(ElasticIndexSettingsDocument document, string indexName)
    {
        // The name "standardanalyzer" is a legacy used prior OC 1.6 release. It can be removed in future releases.
        if (document.ElasticIndexSettings.TryGetValue(indexName, out var settings) && settings.AnalyzerName != "standardanalyzer")
        {
            return settings.AnalyzerName;
        }

        return ElasticsearchConstants.DefaultAnalyzer;
    }
}
