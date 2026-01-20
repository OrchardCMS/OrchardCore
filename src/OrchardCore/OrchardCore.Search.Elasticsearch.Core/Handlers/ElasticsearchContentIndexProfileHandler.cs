using Elastic.Clients.Elasticsearch.Mapping;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Search.Elasticsearch.Core.Mappings;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Handlers;

public sealed class ElasticsearchContentIndexProfileHandler : IndexProfileHandlerBase
{
    private const string _idsPostfixPattern = "*" + ContentIndexingConstants.IdsKey;
    private const string _inheritedPostfixPattern = "*" + ContentIndexingConstants.InheritedKey;
    private const string _locationPostFixPattern = "*.Location";

    public override Task InitializingAsync(InitializingContext<IndexProfile> context)
       => PopulateAsync(context.Model);

    public override Task UpdatingAsync(UpdatingContext<IndexProfile> context)
        => PopulateAsync(context.Model);

    private static Task PopulateAsync(IndexProfile index)
    {
        if (!CanHandle(index))
        {
            return Task.CompletedTask;
        }

        var metadata = index.As<ElasticsearchIndexMetadata>();

        var map = new ElasticsearchIndexMap()
        {
            KeyFieldName = ContentIndexingConstants.ContentItemIdKey,
            Mapping = new TypeMapping()
            {
                Source = new SourceField
                {
                    Enabled = metadata.StoreSourceData,
                    Excludes = [ContentIndexingConstants.DisplayTextAnalyzedKey],
                },
                Properties = [],
                DynamicTemplates = [],
            },
        };

        PopulateTypeMapping(map.Mapping, metadata.StoreSourceData);

        metadata.IndexMappings = map;

        index.Put(metadata);

        return Task.CompletedTask;
    }

    private static bool CanHandle(IndexProfile index)
    {
        return string.Equals(ElasticsearchConstants.ProviderName, index.ProviderName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(IndexingConstants.ContentsIndexSource, index.Type, StringComparison.OrdinalIgnoreCase);
    }

    private static void PopulateTypeMapping(TypeMapping mapping, bool storeSource)
    {
        if (storeSource)
        {
            mapping.Properties[ContentIndexingConstants.ContentItemIdKey] = new KeywordProperty();
            mapping.Properties[ContentIndexingConstants.ContentItemVersionIdKey] = new KeywordProperty();
            mapping.Properties[ContentIndexingConstants.OwnerKey] = new KeywordProperty();
            mapping.Properties[ContentIndexingConstants.FullTextKey] = new TextProperty();

            mapping.Properties[ContentIndexingConstants.ContainedPartKey] = new ObjectProperty
            {
                Properties = new Properties
                {
                    [nameof(ContainedPartModel.Ids)] = new KeywordProperty(),
                    [nameof(ContainedPartModel.Order)] = new FloatNumberProperty(),
                },
            };

            // We map DisplayText here because we have 3 different fields with it.
            // We can't have Content.ContentItem.DisplayText as it is mapped as an Object in Elasticsearch.
            mapping.Properties[ContentIndexingConstants.DisplayTextKey] = new ObjectProperty
            {
                Properties = new Properties
                {
                    [nameof(DisplayTextModel.Analyzed)] = new TextProperty(),
                    [nameof(DisplayTextModel.Normalized)] = new KeywordProperty(),
                    [nameof(DisplayTextModel.Keyword)] = new KeywordProperty(),
                },
            };

            // We map ContentType as a keyword because else the automatic mapping will break the queries.
            // We need to access it with Content.ContentItem.ContentType as a keyword
            // for the ContentPickerResultProvider(s).
            mapping.Properties[ContentIndexingConstants.ContentTypeKey] = new KeywordProperty();
        }

        mapping.DynamicTemplates ??= [];

        if (!mapping.DynamicTemplates.Any(x => x.Key.Equals(_inheritedPostfixPattern, StringComparison.Ordinal)))
        {
            var inheritedPostfix = new DynamicTemplate() { Mapping = new KeywordProperty() };
            inheritedPostfix.PathMatch = [_inheritedPostfixPattern];
            inheritedPostfix.MatchMappingType = ["string"];
            mapping.DynamicTemplates.Add(new KeyValuePair<string, DynamicTemplate>(_inheritedPostfixPattern, inheritedPostfix));
        }

        if (!mapping.DynamicTemplates.Any(x => x.Key.Equals(_idsPostfixPattern, StringComparison.Ordinal)))
        {
            var idsPostfix = new DynamicTemplate() { Mapping = new KeywordProperty() };
            idsPostfix.PathMatch = [_idsPostfixPattern];
            idsPostfix.MatchMappingType = ["string"];
            mapping.DynamicTemplates.Add(new KeyValuePair<string, DynamicTemplate>(_idsPostfixPattern, idsPostfix));
        }

        if (!mapping.DynamicTemplates.Any(x => x.Key.Equals(_locationPostFixPattern, StringComparison.Ordinal)))
        {
            var locationPostfix = new DynamicTemplate() { Mapping = new GeoPointProperty() };
            locationPostfix.PathMatch = [_locationPostFixPattern];
            locationPostfix.MatchMappingType = ["object"];
            mapping.DynamicTemplates.Add(new KeyValuePair<string, DynamicTemplate>(_locationPostFixPattern, locationPostfix));
        }
    }
}
