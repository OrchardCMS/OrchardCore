using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Services;

public interface IElasticsearchIndexSettingsHandler
{
    Task InitializingAsync(ElasticsearchIndexSettingsInitializingContext context);

    Task CreatingAsync(ElasticsearchIndexSettingsCreateContext context);

    Task CreatedAsync(ElasticsearchIndexSettingsCreateContext context);

    Task UpdatingAsync(ElasticsearchIndexSettingsUpdateContext context);

    Task UpdatedAsync(ElasticsearchIndexSettingsUpdateContext context);

    Task ValidatingAsync(ElasticsearchIndexSettingsValidatingContext context);

    Task ResetAsync(ElasticsearchIndexSettingsResetContext context);

    Task SynchronizedAsync(ElasticsearchIndexSettingsSynchronizedContext context);

    Task SynchronizedSettingsAsync(ElasticsearchIndexSettingsSynchronizedSettingsContext context);

    Task ExportingAsync(ElasticsearchIndexSettingsExportingContext context);
}
