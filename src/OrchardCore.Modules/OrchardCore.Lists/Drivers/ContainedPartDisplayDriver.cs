using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;
using OrchardCore.Lists.ViewModels;
using YesSql;

namespace OrchardCore.Lists.Drivers
{
    public class ContainedPartDisplayDriver : ContentDisplayDriver
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IContainerService _containerService;

        public ContainedPartDisplayDriver(
            IContentManager contentManager,
            ISession session,
            IContainerService containerService
            )
        {
            _session = session;
            _contentManager = contentManager;
            _containerService = containerService;
        }

        public override async Task<IDisplayResult> EditAsync(ContentItem model, IUpdateModel updater)
        {
            // This method can get called when a new content item is created, at that point
            // the query string contains a ListPart.ContainerId value, or when an
            // existing content item has ContainedPart value. In both cases the hidden field
            // needs to be rendered in the edit form to maintain the relationship with the parent.

            if (model.As<ContainedPart>() != null)
            {
                return BuildViewModel(model.As<ContainedPart>().ListContentItemId, model.ContentType);
            }

            var viewModel = new EditContainedPartViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, nameof(ListPart)) && viewModel.ContainerId != null && viewModel.ContentType == model.ContentType)
            {
                // We are creating a content item that needs to be added to a container
                // so we render the container id as part of the form, the content type,
                // and the enable ordering setting.
                // The content type must be included to prevent any contained items,
                // such as widgets, from also having a ContainedPart shape built for them.

                return BuildViewModel(viewModel.ContainerId, model.ContentType, viewModel.EnableOrdering);
            }

            return null;
        }

        private IDisplayResult BuildViewModel(string containerId, string contentType, bool enableOrdering = false)
        {
            return Initialize<EditContainedPartViewModel>("ListPart_ContainerId", m =>
            {
                m.ContainerId = containerId;
                m.EnableOrdering = enableOrdering;
                m.ContentType = contentType;
            })
            .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItem model, IUpdateModel updater)
        {
            var viewModel = new EditContainedPartViewModel();

            // The content type must match the value provided in the query string
            // in order for the ContainedPart to be included on the Content Item.
            if (await updater.TryUpdateModelAsync(viewModel, nameof(ListPart)) && viewModel.ContainerId != null && viewModel.ContentType == model.ContentType)
            {
                model.Alter<ContainedPart>(x => x.ListContentItemId = viewModel.ContainerId);
                // If creating get next order number so item is added to the end of the list
                if (viewModel.EnableOrdering)
                {
                    var nextOrder = await _containerService.GetNextOrderNumberAsync(viewModel.ContainerId);
                    model.Alter<ContainedPart>(x => x.Order = nextOrder);
                }
            }

            return await EditAsync(model, updater);
        }
    }
}
