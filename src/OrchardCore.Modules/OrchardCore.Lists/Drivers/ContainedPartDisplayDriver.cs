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
                return BuildShape(model.As<ContainedPart>().ListContentItemId);
            }

            var viewModel = new EditContainedPartViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, nameof(ListPart)) && viewModel.ContainerId != null)
            {
                // We are creating a content item that needs to be added to a container
                // so we render the container id as part of the form, and
                // the enable ordering setting

                return BuildShape(viewModel.ContainerId, viewModel.EnableOrdering);
            }

            return null;
        }

        private IDisplayResult BuildShape(string containerId, bool enableOrdering = false)
        {
            return Dynamic("ListPart_ContainerId", shape =>
            {
                shape.ContainerId = containerId;
                shape.EnableOrdering = enableOrdering;
            })
            .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItem model, IUpdateModel updater)
        {
            var viewModel = new EditContainedPartViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, nameof(ListPart)) && viewModel.ContainerId != null)
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
