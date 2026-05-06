using OpenSearch.Client;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.OpenSearch.Core.Mappings;
using OrchardCore.OpenSearch.Core.Models;
using OrchardCore.OpenSearch.Models;

namespace OrchardCore.OpenSearch.Core.Handlers;

public sealed class OpenSearchContentIndexProfileHandler : IndexProfileHandlerBase
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

        var metadata = index.As<OpenSearchIndexMetadata>();

        var map = new OpenSearchIndexMap()
        {
            KeyFieldName = ContentIndexingConstants.ContentItemIdKey,
            Mapping = new TypeMapping()
            {
                Properties = new Properties(),
                DynamicTemplates = new DynamicTemplateContainer(),
            },
        };

        PopulateTypeMapping(map.Mapping, metadata.StoreSourceData);

        metadata.IndexMappings = map;

        index.Put(metadata);

        return Task.CompletedTask;
    }

    private static bool CanHandle(IndexProfile index)
    {
        return string.Equals(OpenSearchConstants.ProviderName, index.ProviderName, StringComparison.OrdinalIgnoreCase) &&
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
                    [nameof(ContainedPartModel.Order)] = new NumberProperty(NumberType.Float),
                },
            };

            mapping.Properties[ContentIndexingConstants.DisplayTextKey] = new ObjectProperty
            {
                Properties = new Properties
                {
                    [nameof(DisplayTextModel.Analyzed)] = new TextProperty(),
                    [nameof(DisplayTextModel.Normalized)] = new KeywordProperty(),
                    [nameof(DisplayTextModel.Keyword)] = new KeywordProperty(),
                },
            };

            mapping.Properties[ContentIndexingConstants.ContentTypeKey] = new KeywordProperty();
        }

        mapping.DynamicTemplates ??= new DynamicTemplateContainer();

        if (!mapping.DynamicTemplates.ContainsKey(_inheritedPostfixPattern))
        {
            mapping.DynamicTemplates[_inheritedPostfixPattern] = new DynamicTemplate
            {
                Mapping = new KeywordProperty(),
                PathMatch = _inheritedPostfixPattern,
                MatchMappingType = "string",
            };
        }

        if (!mapping.DynamicTemplates.ContainsKey(_idsPostfixPattern))
        {
            mapping.DynamicTemplates[_idsPostfixPattern] = new DynamicTemplate
            {
                Mapping = new KeywordProperty(),
                PathMatch = _idsPostfixPattern,
                MatchMappingType = "string",
            };
        }

        if (!mapping.DynamicTemplates.ContainsKey(_locationPostFixPattern))
        {
            mapping.DynamicTemplates[_locationPostFixPattern] = new DynamicTemplate
            {
                Mapping = new GeoPointProperty(),
                PathMatch = _locationPostFixPattern,
                MatchMappingType = "object",
            };
        }
    }
}
