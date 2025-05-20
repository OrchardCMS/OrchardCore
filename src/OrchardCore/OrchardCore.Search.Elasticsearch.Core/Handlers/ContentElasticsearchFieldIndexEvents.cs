using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.Elasticsearch.Core.Mappings;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Handlers;

public sealed class ContentElasticsearchFieldIndexEvents : ElasticsearchIndexSettingsHandlerBase
{
    private const string _idsPostfixPattern = "*" + IndexingConstants.IdsKey;
    private const string _inheritedPostfixPattern = "*" + IndexingConstants.InheritedKey;
    private const string _locationPostFixPattern = "*.Location";

    private readonly IServiceProvider _serviceProvider;

    public ContentElasticsearchFieldIndexEvents(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override Task UpdatingAsync(ElasticsearchIndexSettingsUpdateContext context)
    {
        if (context.Settings.Source != ElasticsearchConstants.ContentsIndexSource)
        {
            return Task.CompletedTask;
        }

        AddMappings(context.Settings);

        return Task.CompletedTask;
    }

    public override Task CreatingAsync(ElasticsearchIndexSettingsCreateContext context)
    {
        if (context.Settings.Source != ElasticsearchConstants.ContentsIndexSource)
        {
            return Task.CompletedTask;
        }

        AddMappings(context.Settings);

        return Task.CompletedTask;
    }

    public override Task SynchronizedSettingsAsync(ElasticsearchIndexSettingsSynchronizedSettingsContext context)
    {
        // Lasley load the indexing service to avoid circular dependency issues.

        var indexingService = _serviceProvider.GetRequiredService<ElasticsearchContentIndexingService>();

        return indexingService.SyncSettingsAsync();
    }

    internal static void AddMappings(ElasticIndexSettings settings)
    {
        settings.IndexMappings.Source ??= new SourceField();
        settings.IndexMappings.Source.Enabled = settings.As<ContentIndexMetadata>().StoreSourceData;
        settings.IndexMappings.Source.Excludes = [IndexingConstants.DisplayTextAnalyzedKey];

        settings.IndexMappings.Properties ??= new Properties();
        settings.IndexMappings.Properties[IndexingConstants.ContentItemIdKey] = new KeywordProperty();
        settings.IndexMappings.Properties[IndexingConstants.ContentItemVersionIdKey] = new KeywordProperty();
        settings.IndexMappings.Properties[IndexingConstants.OwnerKey] = new KeywordProperty();
        settings.IndexMappings.Properties[IndexingConstants.FullTextKey] = new TextProperty();

        settings.IndexMappings.Properties[IndexingConstants.ContainedPartKey] = new ObjectProperty
        {
            Properties = new Properties
            {
                [nameof(ContainedPartModel.Ids)] = new KeywordProperty(),
                [nameof(ContainedPartModel.Order)] = new FloatNumberProperty(),
            },
        };

        // We map DisplayText here because we have 3 different fields with it.
        // We can't have Content.ContentItem.DisplayText as it is mapped as an Object in Elasticsearch.
        settings.IndexMappings.Properties[IndexingConstants.DisplayTextKey] = new ObjectProperty
        {
            Properties = new Properties
            {
                [nameof(DisplayTextModel.Analyzed)] = new TextProperty(),
                [nameof(DisplayTextModel.Normalized)] = new KeywordProperty(),
                [nameof(DisplayTextModel.Keyword)] = new KeywordProperty(),
            },
        };

        // We map ContentType as a keyword because else the automatic settings.IndexMappings will break the queries.
        // We need to access it with Content.ContentItem.ContentType as a keyword
        // for the ContentPickerResultProvider(s).
        settings.IndexMappings.Properties[IndexingConstants.ContentTypeKey] = new KeywordProperty();

        settings.IndexMappings.DynamicTemplates ??= [];

        if (!settings.IndexMappings.DynamicTemplates.Any(x => x.Key == _inheritedPostfixPattern))
        {
            var inheritedPostfix = new DynamicTemplate
            {
                Mapping = new KeywordProperty(),
                PathMatch = [_inheritedPostfixPattern],
                MatchMappingType = ["string"],
            };

            settings.IndexMappings.DynamicTemplates.Add(new KeyValuePair<string, DynamicTemplate>(_inheritedPostfixPattern, inheritedPostfix));
        }

        if (!settings.IndexMappings.DynamicTemplates.Any(x => x.Key == _idsPostfixPattern))
        {
            var idsPostfix = new DynamicTemplate
            {
                Mapping = new KeywordProperty(),
                PathMatch = [_idsPostfixPattern],
                MatchMappingType = ["string"],
            };

            settings.IndexMappings.DynamicTemplates.Add(new KeyValuePair<string, DynamicTemplate>(_idsPostfixPattern, idsPostfix));
        }

        if (!settings.IndexMappings.DynamicTemplates.Any(x => x.Key == _locationPostFixPattern))
        {
            var locationPostfix = new DynamicTemplate
            {
                Mapping = new GeoPointProperty(),
                PathMatch = [_locationPostFixPattern],
                MatchMappingType = ["object"],
            };

            settings.IndexMappings.DynamicTemplates.Add(new KeyValuePair<string, DynamicTemplate>(_locationPostFixPattern, locationPostfix));
        }
    }
}
