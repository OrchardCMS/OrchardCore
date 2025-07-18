using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Flows.Models;
using OrchardCore.Indexing;
using OrchardCore.Search.AzureAI.Handlers;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Flows.Handlers;

internal sealed class BagPartAzureAISearchFieldIndexEvents : AzureAISearchFieldIndexEventsBase
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IServiceProvider _serviceProvider;

    private AzureAISearchContentFieldMapper _fieldsMapper;

    public BagPartAzureAISearchFieldIndexEvents(
        IContentDefinitionManager contentDefinitionManager,
        IServiceProvider serviceProvider)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _serviceProvider = serviceProvider;
    }

    public override async Task MappingAsync(SearchIndexDefinition context)
    {
        // Lazily resolve the handler to avoid circular dependencies.
        _fieldsMapper ??= _serviceProvider.GetService<AzureAISearchContentFieldMapper>();

        if (context.IndexEntry.Type == DocumentIndex.Types.Complex &&
            context.IndexEntry.Metadata?.TryGetValue(nameof(ContentTypePartDefinition), out var definition) == true &&
            definition is ContentTypePartDefinition contentTypePartDefinition && contentTypePartDefinition.PartDefinition.Name == nameof(BagPart))
        {
            context.Map.SubFields ??= new List<AzureAISearchIndexMap>();

            var contentItemsField = context.Map.SubFields.FirstOrDefault(x => x.AzureFieldKey == "ContentItems");

            if (contentItemsField is null)
            {
                contentItemsField = new AzureAISearchIndexMap("ContentItems", DocumentIndex.Types.Complex)
                {
                    IsCollection = true,
                    SubFields = new List<AzureAISearchIndexMap>(),
                };

                context.Map.SubFields.Add(contentItemsField);
            }

            var settings = contentTypePartDefinition.GetSettings<BagPartSettings>();

            var contentTypes = new List<string>();

            if (settings.ContainedContentTypes is not null && settings.ContainedContentTypes.Length > 0)
            {
                contentTypes.AddRange(settings.ContainedContentTypes);
            }
            else if (settings.ContainedStereotypes is not null && settings.ContainedStereotypes.Length > 0)
            {
                var types = await _contentDefinitionManager.ListTypeDefinitionsAsync();

                contentTypes.AddRange(types.Where(x => settings.ContainedStereotypes.Any(c => x.StereotypeEquals(c))).Select(x => x.Name));
            }

            await _fieldsMapper.MapAsync(contentItemsField.SubFields, context.IndexProfile, contentTypes, rootFields: false);
        }
    }
}
