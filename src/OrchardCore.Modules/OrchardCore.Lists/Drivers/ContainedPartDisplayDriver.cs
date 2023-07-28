using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;
using OrchardCore.Lists.ViewModels;

namespace OrchardCore.Lists.Drivers
{
    public class ContainedPartDisplayDriver : ContentDisplayDriver
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

        public override async Task<IDisplayResult> EditAsync(ContentItem model, IUpdateModel updater)
        {
            // This method can get called when a new content item is created, at that point
            // the query string contains a ListPart.ContainerId value, or when an
            // existing content item has ContainedPart value. In both cases the hidden field
            // needs to be rendered in the edit form to maintain the relationship with the parent.
            var containedPart = model.As<ContainedPart>();

            if (containedPart != null)
            {
                return BuildViewModel(containedPart.ListContentItemId, containedPart.ListContentType, model.ContentType);
            }

            var viewModel = new EditContainedPartViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, nameof(ListPart))
                && viewModel.ContainerId != null
                && viewModel.ContentType == model.ContentType)
            {
                // We are creating a content item that needs to be added to a container.
                // Render the container id as part of the form. The content type, and the enable ordering setting.
                // The content type must be included to prevent any contained items,
                // such as widgets, from also having a ContainedPart shape built for them.

                // Attach ContainedPart to the contentitem during edit to provide handlers container info.
                await model.AlterAsync<ContainedPart>(async part =>
                {
                    part.ListContentItemId = viewModel.ContainerId;
                    part.ListContentType = viewModel.ContainerContentType;
                    if (viewModel.EnableOrdering)
                    {
                        part.Order = await _containerService.GetNextOrderNumberAsync(viewModel.ContainerId);
                    }
                });

                return BuildViewModel(viewModel.ContainerId, viewModel.ContainerContentType, model.ContentType, viewModel.EnableOrdering);
            }

            return null;
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItem model, IUpdateModel updater)
        {
            var viewModel = new EditContainedPartViewModel();

            // The content type must match the value provided in the query string
            // in order for the ContainedPart to be included on the Content Item.
            if (await updater.TryUpdateModelAsync(viewModel, nameof(ListPart))
                && viewModel.ContainerId != null
                && viewModel.ContentType == model.ContentType)
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

            return await EditAsync(model, updater);
        }

        private IDisplayResult BuildViewModel(string containerId, string containerContentType, string contentType, bool enableOrdering = false)
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

            if (!String.IsNullOrEmpty(containerContentType))
            {
                var definition = _contentDefinitionManager.GetTypeDefinition(containerContentType);

                if (definition != null)
                {
                    var listPart = definition.Parts.FirstOrDefault(x => x.PartDefinition.Name == nameof(ListPart));
                    var settings = listPart?.GetSettings<ListPartSettings>();

                    if (settings != null)
                    {
                        // Add list part navigation
                        results.Add(Initialize<ListPartNavigationAdminViewModel>("ListPartNavigationAdmin", async model =>
                        {
                            model.ContainedContentTypeDefinitions = GetContainedContentTypes(settings).ToArray();
                            model.Container = await _contentManager.GetAsync(containerId, VersionOptions.Latest);
                            model.EnableOrdering = settings.EnableOrdering;
                            model.ContainerContentTypeDefinition = definition;
                        }).Location("Content:1.5"));

                        if (settings.ShowHeader)
                        {
                            results.Add(GetListPartHeader(containerId, settings));
                        }
                    }
                }
            }

            return Combine(results);
        }

        private IDisplayResult GetListPartHeader(string containerId, ListPartSettings listPartSettings)
        {
            return Initialize<ListPartHeaderAdminViewModel>("ListPartHeaderAdmin", async model =>
            {
                var container = await _contentManager.GetAsync(containerId);

                if (container == null)
                {
                    return;
                }

                model.ContainerContentItem = container;

                if (listPartSettings != null)
                {
                    model.ContainedContentTypeDefinitions = GetContainedContentTypes(listPartSettings).ToArray();
                    model.EnableOrdering = listPartSettings.EnableOrdering;
                }
            }).Location("Content:1");
        }

        private IEnumerable<ContentTypeDefinition> GetContainedContentTypes(ListPartSettings settings) =>
            settings.ContainedContentTypes
                ?.Select(contentType => _contentDefinitionManager.GetTypeDefinition(contentType))
                .Where(definition => definition is not null)
                ?? Enumerable.Empty<ContentTypeDefinition>();
    }
}
