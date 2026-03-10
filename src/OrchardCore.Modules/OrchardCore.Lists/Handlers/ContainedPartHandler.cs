using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;

namespace OrchardCore.Lists.Handlers;

public class ContainedPartHandler : ContentHandlerBase
{
    private readonly IServiceProvider _serviceProvider;

    internal readonly IStringLocalizer S;

    private HashSet<string> _containedContentTypes;

    public ContainedPartHandler(
        IServiceProvider serviceProvider,
        IStringLocalizer<ContainedPartHandler> localizer)
    {
        _serviceProvider = serviceProvider;
        S = localizer;
    }

    public override async Task ValidatingAsync(ValidateContentContext context)
    {
        var contentType = context.ContentItem.ContentType;

        var containedContentTypes = await GetContainedContentTypesAsync();

        if (!containedContentTypes.TryGetValue(contentType, out _))
        {
            return;
        }

        var contentDefinitionManager = _serviceProvider.GetRequiredService<IContentDefinitionManager>();

        var definition = await contentDefinitionManager.GetTypeDefinitionAsync(contentType);

        if (definition.IsCreatable())
        {
            // If the content type is creatable, it means it can be created outside of a list, so we don't require the ListContentItemId to be set.
            return;
        }

        if (!context.ContentItem.TryGet<ContainedPart>(out var containedPart))
        {
            return;
        }

        if (string.IsNullOrEmpty(containedPart.ListContentItemId))
        {
            if (string.IsNullOrEmpty(containedPart.ListContentType))
            {
                return;
            }

            context.Fail(S["The content item of type '{0}' must have a valid ListContentItemId as it is contained by a list.", contentType], nameof(ContainedPart.ListContentItemId));
        }
    }

    private async Task<HashSet<string>> GetContainedContentTypesAsync()
    {
        if (_containedContentTypes == null)
        {
            // Resolve from DI to avoid circular references.
            var contentDefinitionManager = _serviceProvider.GetRequiredService<IContentDefinitionManager>();
            var typeDefinitions = await contentDefinitionManager.ListTypeDefinitionsAsync();

            _containedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var typeDefinition in typeDefinitions)
            {
                var listPartDefinition = typeDefinition.Parts.FirstOrDefault(p =>
                    string.Equals(p.PartDefinition.Name, nameof(ListPart), StringComparison.Ordinal));

                if (listPartDefinition == null)
                {
                    continue;
                }

                var settings = listPartDefinition.GetSettings<ListPartSettings>();

                if (settings.ContainedContentTypes != null)
                {
                    foreach (var containedContentType in settings.ContainedContentTypes)
                    {
                        if (string.IsNullOrEmpty(containedContentType))
                        {
                            continue;
                        }

                        _containedContentTypes.Add(containedContentType);
                    }
                }
            }
        }

        return _containedContentTypes;
    }

    public override async Task CloningAsync(CloneContentContext context)
    {
        var containedPart = context.CloneContentItem.As<ContainedPart>();
        if (containedPart != null)
        {
            // Resolve from DI to avoid circular references.
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            var listContentItem = await contentManager.GetAsync(containedPart.ListContentItemId);
            if (listContentItem != null)
            {
                var contentDefinitionManager = _serviceProvider.GetRequiredService<IContentDefinitionManager>();
                var contentTypeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync(listContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, "ListPart", StringComparison.Ordinal));
                var settings = contentTypePartDefinition.GetSettings<ListPartSettings>();
                if (settings.EnableOrdering)
                {
                    var containerService = _serviceProvider.GetRequiredService<IContainerService>();
                    var nextOrder = await containerService.GetNextOrderNumberAsync(containedPart.ListContentItemId);
                    context.CloneContentItem.Alter<ContainedPart>(x => x.Order = nextOrder);
                }
            }
        }
    }
}
