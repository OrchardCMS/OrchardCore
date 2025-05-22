using OrchardCore.Search.Elasticsearch;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Models;
using OrchardCore.Search.Elasticsearch.Services;

namespace OrchardCore.Search.AzureAI.Services;

public abstract class ElasticsearchIndexSettingsHandlerBase : IElasticsearchIndexSettingsHandler
{
    public virtual Task CreatedAsync(ElasticsearchIndexSettingsCreateContext context)
        => Task.CompletedTask;

    public virtual Task CreatingAsync(ElasticsearchIndexSettingsCreateContext context)
        => Task.CompletedTask;

    public virtual Task ExportingAsync(ElasticsearchIndexSettingsExportingContext context)
        => Task.CompletedTask;

    public virtual Task InitializingAsync(ElasticsearchIndexSettingsInitializingContext context)
        => Task.CompletedTask;

    public virtual Task ResetAsync(ElasticsearchIndexSettingsResetContext context)
        => Task.CompletedTask;

    public virtual Task SynchronizedAsync(ElasticsearchIndexSettingsSynchronizedContext context)
        => Task.CompletedTask;

    public virtual Task SynchronizedSettingsAsync(ElasticsearchIndexSettingsSynchronizedSettingsContext context)
        => Task.CompletedTask;

    public virtual Task UpdatingAsync(ElasticsearchIndexSettingsUpdateContext context)
        => Task.CompletedTask;

    public virtual Task UpdatedAsync(ElasticsearchIndexSettingsUpdateContext context)
        => Task.CompletedTask;

    public virtual Task ValidatingAsync(ElasticsearchIndexSettingsValidatingContext context)
        => Task.CompletedTask;

    protected static bool CanHandle(ElasticIndexSettings settings)
    {
        return string.Equals(ElasticsearchConstants.ContentsIndexSource, settings.Source, StringComparison.OrdinalIgnoreCase);
    }
}
