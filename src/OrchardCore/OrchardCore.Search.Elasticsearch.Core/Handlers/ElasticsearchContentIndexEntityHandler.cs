using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch.Mapping;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Elasticsearch.Core.Mappings;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Handlers;

public sealed class ElasticsearchContentIndexEntityHandler : IndexEntityHandlerBase
{
    private const string _idsPostfixPattern = "*" + ContentIndexingConstants.IdsKey;
    private const string _inheritedPostfixPattern = "*" + ContentIndexingConstants.InheritedKey;
    private const string _locationPostFixPattern = "*.Location";

    public override Task InitializingAsync(InitializingContext<IndexEntity> context)
       => PopulateAsync(context.Model, context.Data);

    private static Task PopulateAsync(IndexEntity index, JsonNode data)
    {
        if (!CanHandle(index))
        {
            return Task.CompletedTask;
        }

        var metadata = index.As<ElasticsearchIndexMetadata>();

        metadata.KeyFieldName = ContentIndexingConstants.ContentItemIdKey;

        var mapping = new ElasticsearchIndexMap()
        {
            SourceField = new SourceField
            {
                Enabled = index.As<ElasticsearchContentIndexMetadata>().StoreSourceData,
                Excludes = [ContentIndexingConstants.DisplayTextAnalyzedKey],
            },
        };

        PopulateTypeMapping(mapping);

        metadata.IndexMappings = mapping;

        index.Put(metadata);

        var contentMetadata = index.As<ElasticsearchContentIndexMetadata>();

        var storeSourceData = data[nameof(contentMetadata.StoreSourceData)]?.GetValue<bool>();

        if (storeSourceData.HasValue)
        {
            contentMetadata.StoreSourceData = storeSourceData.Value;
        }

        index.Put(contentMetadata);

        return Task.CompletedTask;
    }

    private static bool CanHandle(IndexEntity index)
    {
        return string.Equals(ElasticsearchConstants.ProviderName, index.ProviderName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(IndexingConstants.ContentsIndexSource, index.Type, StringComparison.OrdinalIgnoreCase);
    }

    private static void PopulateTypeMapping(ElasticsearchIndexMap mapping)
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

        if (!mapping.DynamicTemplates.Any(x => x.ContainsKey(_inheritedPostfixPattern)))
        {
            var inheritedPostfix = DynamicTemplate.Mapping(new KeywordProperty());
            inheritedPostfix.PathMatch = [_inheritedPostfixPattern];
            inheritedPostfix.MatchMappingType = ["string"];
            mapping.DynamicTemplates.Add(new Dictionary<string, DynamicTemplate>()
            {
                { _inheritedPostfixPattern, inheritedPostfix },
            });
        }

        if (!mapping.DynamicTemplates.Any(x => x.ContainsKey(_idsPostfixPattern)))
        {
            var idsPostfix = DynamicTemplate.Mapping(new KeywordProperty());
            idsPostfix.PathMatch = [_idsPostfixPattern];
            idsPostfix.MatchMappingType = ["string"];
            mapping.DynamicTemplates.Add(new Dictionary<string, DynamicTemplate>()
            {
                { _idsPostfixPattern, idsPostfix },
            });
        }

        if (!mapping.DynamicTemplates.Any(x => x.ContainsKey(_locationPostFixPattern)))
        {
            var locationPostfix = DynamicTemplate.Mapping(new GeoPointProperty());
            locationPostfix.PathMatch = [_locationPostFixPattern];
            locationPostfix.MatchMappingType = ["object"];
            mapping.DynamicTemplates.Add(new Dictionary<string, DynamicTemplate>()
            {
                { _locationPostFixPattern, locationPostfix },
            });
            mapping.DynamicTemplates.Add(new Dictionary<string, DynamicTemplate>()
            {
                { _locationPostFixPattern, locationPostfix },
            });
        }
    }
}
