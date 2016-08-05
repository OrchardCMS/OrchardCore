using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Lists.Models;
using Orchard.Lists.ViewModels;
using YesSql.Core.Services;

namespace Orchard.Lists.Drivers
{
    public class ContainedPartDisplayDriver : ContentDisplayDriver
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public ContainedPartDisplayDriver(
            IContentManager contentManager,
            ISession session
            )
        {
            _session = session;
            _contentManager = contentManager;
        }

        public override async Task<IDisplayResult> EditAsync(ContentItem model, IUpdateModel updater)
        {
            // This method can get called when a new content item is created, at that point
            // the query string contains a ListPart.ContainerId value, or when an
            // existing content item has ContainedPart value. In both cases the hidden field
            // needs to be rendered in the edit form to maintain the relationship with the parent.

            if (model.Is<ContainedPart>())
            {
                return BuildShape(model.As<ContainedPart>().ListContentItemId);
            }

            var viewModel = new EditContainedPartViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, nameof(ListPart)))
            {
                // We are editing a content item that needs to be added to a container
                // so we render the container id as part of the form

                return BuildShape(viewModel.ContainerId);
            }

            return null;
        }

        private IDisplayResult BuildShape(int containerId)
        {
            return Shape("ListPart_ContainerId", shape =>
            {
                shape.ContainerId = containerId;
                return Task.CompletedTask;
            })
            .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItem model, IUpdateModel updater)
        {
            var viewModel = new EditContainedPartViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, nameof(ListPart)))
            {
                model.Weld<ContainedPart>();
                model.Alter<ContainedPart>(x => x.ListContentItemId = viewModel.ContainerId);
            }

            return await base.UpdateAsync(model, updater);
        }
    }
}