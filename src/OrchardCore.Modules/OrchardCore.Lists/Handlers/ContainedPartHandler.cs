using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;

namespace OrchardCore.Lists.Handlers;

public class ContainedPartHandler : ContentHandlerBase
{
    private readonly IServiceProvider _serviceProvider;

    internal readonly IStringLocalizer S;

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

        // Resolve from DI to avoid circular references.
        var contentDefinitionManager = _serviceProvider.GetRequiredService<IContentDefinitionManager>();
        var allTypeDefinitions = await contentDefinitionManager.ListTypeDefinitionsAsync();

        var isContainedType = allTypeDefinitions.Any(typeDef =>
        {
            var listPartDefinition = typeDef.Parts.FirstOrDefault(p =>
                string.Equals(p.PartDefinition.Name, nameof(ListPart), StringComparison.Ordinal));

            if (listPartDefinition == null)
            {
                return false;
            }

            var settings = listPartDefinition.GetSettings<ListPartSettings>();

            return settings.ContainedContentTypes?.Contains(contentType) == true;
        });

        if (!isContainedType)
        {
            return;
        }

        var containedPart = context.ContentItem.As<ContainedPart>();

        if (containedPart == null)
        {
            context.Fail(S["The content item of type '{0}' must be associated with a list via ContainedPart.", contentType], nameof(ContainedPart));
            return;
        }

        if (string.IsNullOrEmpty(containedPart.ListContentItemId))
        {
            context.Fail(S["The content item of type '{0}' must have a valid ListContentItemId as it is contained by a list.", contentType], nameof(ContainedPart.ListContentItemId));
        }

        if (string.IsNullOrEmpty(containedPart.ListContentType))
        {
            context.Fail(S["The content item of type '{0}' must have a valid ListContentType as it is contained by a list.", contentType], nameof(ContainedPart.ListContentType));
        }
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
