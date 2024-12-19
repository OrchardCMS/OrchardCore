using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;
using OrchardCore.Lists.ViewModels;

namespace OrchardCore.Lists.Drivers;

public sealed class ContainedPartDisplayDriver : ContentDisplayDriver
{
    private readonly IContentManager _contentManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContainerService _containerService;

    public ContainedPartDisplayDriver(
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager,
        IContainerService containerService
        )
    {
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
        _containerService = containerService;
    }

    public override async Task<IDisplayResult> EditAsync(ContentItem model, BuildEditorContext context)
    {
        // This method can get called when a new content item is created, at that point
        // the query string contains a ListPart.ContainerId value, or when an
        // existing content item has ContainedPart value. In both cases the hidden field
        // needs to be rendered in the edit form to maintain the relationship with the parent.
        var containedPart = model.As<ContainedPart>();

        if (containedPart != null)
        {
            return await BuildViewModelAsync(containedPart.ListContentItemId, containedPart.ListContentType, model.ContentType);
        }

        var viewModel = new EditContainedPartViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, nameof(ListPart));

        if (viewModel.ContainerId != null && viewModel.ContentType == model.ContentType)
        {
            // We are creating a content item that needs to be added to a container.
            // Render the container id as part of the form. The content type, and the enable ordering setting.
            // The content type must be included to prevent any contained items,
            // such as widgets, from also having a ContainedPart shape built for them.

            // Attach ContainedPart to the content item during edit to provide handlers container info.
            await model.AlterAsync<ContainedPart>(async part =>
            {
                part.ListContentItemId = viewModel.ContainerId;
                part.ListContentType = viewModel.ContainerContentType;
                if (viewModel.EnableOrdering)
                {
                    part.Order = await _containerService.GetNextOrderNumberAsync(viewModel.ContainerId);
                }
            });

            return await BuildViewModelAsync(viewModel.ContainerId, viewModel.ContainerContentType, model.ContentType, viewModel.EnableOrdering);
        }

        return null;
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentItem model, UpdateEditorContext context)
    {
        var viewModel = new EditContainedPartViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, nameof(ListPart));

        // The content type must match the value provided in the query string
        // in order for the ContainedPart to be included on the Content Item.
        if (viewModel.ContainerId != null && viewModel.ContentType == model.ContentType)
        {
            await model.AlterAsync<ContainedPart>(async part =>
            {
                part.ListContentItemId = viewModel.ContainerId;
                part.ListContentType = viewModel.ContainerContentType;

                // If creating get next order number so item is added to the end of the list.
                if (viewModel.EnableOrdering)
                {
                    part.Order = await _containerService.GetNextOrderNumberAsync(viewModel.ContainerId);
                }
            });
        }

        return await EditAsync(model, context);
    }

    private async Task<IDisplayResult> BuildViewModelAsync(string containerId, string containerContentType, string contentType, bool enableOrdering = false)
    {
        var results = new List<IDisplayResult>()
        {
            Initialize<EditContainedPartViewModel>("ListPart_ContainerId", m =>
            {
                m.ContainerId = containerId;
                m.ContainerContentType = containerContentType;
                m.EnableOrdering = enableOrdering;
                m.ContentType = contentType;
            })
            .Location("Content"),
        };

        if (!string.IsNullOrEmpty(containerContentType))
        {
            var definition = await _contentDefinitionManager.GetTypeDefinitionAsync(containerContentType);

            if (definition != null)
            {
                var listPart = definition.Parts.FirstOrDefault(x => x.PartDefinition.Name == nameof(ListPart));
                var settings = listPart?.GetSettings<ListPartSettings>();

                if (settings != null)
                {
                    var container = await GetContainerAsync(containerId);

                    if (container != null)
                    {
                        // Add list part navigation.
                        results.Add(Initialize<ListPartNavigationAdminViewModel>("ListPartNavigationAdmin", async model =>
                        {
                            model.ContainedContentTypeDefinitions = (await GetContainedContentTypesAsync(settings)).ToArray();
                            model.Container = container;
                            model.EnableOrdering = settings.EnableOrdering;
                            model.ContainerContentTypeDefinition = definition;
                        }).Location("Content:1.5"));

                        if (settings.ShowHeader)
                        {
                            results.Add(GetListPartHeader(container, settings));
                        }
                    }
                }
            }
        }

        return Combine(results);
    }

    private ShapeResult GetListPartHeader(ContentItem containerContentItem, ListPartSettings listPartSettings)
        => Initialize<ListPartHeaderAdminViewModel>("ListPartHeaderAdmin", async model =>
        {
            model.ContainerContentItem = containerContentItem;

            if (listPartSettings != null)
            {
                model.ContainedContentTypeDefinitions = (await GetContainedContentTypesAsync(listPartSettings)).ToArray();
                model.EnableOrdering = listPartSettings.EnableOrdering;
            }
        }).Location("Content:1");

    // Initially, attempt to locate a published container.
    // If none is found, try acquiring the most recent unpublished version.
    private async Task<ContentItem> GetContainerAsync(string containerId)
        => await _contentManager.GetAsync(containerId) ?? await _contentManager.GetAsync(containerId, VersionOptions.Latest);

    private async Task<IEnumerable<ContentTypeDefinition>> GetContainedContentTypesAsync(ListPartSettings settings)
    {
        if (settings.ContainedContentTypes == null)
        {
            return [];
        }

        var definitions = new List<ContentTypeDefinition>();

        foreach (var contentTypeDefinition in settings.ContainedContentTypes)
        {
            var definition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentTypeDefinition);

            if (definition is not null)
            {
                definitions.Add(definition);
            }
        }

        return definitions;
    }
}
