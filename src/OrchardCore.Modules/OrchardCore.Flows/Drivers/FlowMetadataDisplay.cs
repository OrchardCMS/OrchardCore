using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.ViewModels;

namespace OrchardCore.Flows.Drivers
{
    public class FlowMetadataDisplay : ContentDisplayDriver
    {
        public override IDisplayResult Edit(ContentItem contentItem, IUpdateModel updater)
        {
            var model = contentItem.As<FlowMetadata>();

            if (model == null)
            {
                return null;
            }

            return Initialize<FlowMetadataEditViewModel>("FlowMetadata_Edit", m =>
            {
                m.Alignment = model.Alignment;
                m.Size = model.Size;
                m.Classes = String.Join(" ", model.Classes);
            }).Location("Footer");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItem contentItem, IUpdateModel updater)
        {
            var model = contentItem.As<FlowMetadata>();

            if (model == null)
            {
                return null;
            }

            var viewModel = new FlowMetadataEditViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                model.Alignment = viewModel.Alignment;
                model.Size = viewModel.Size;
                model.Classes = viewModel.Classes?.Split(" ", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                contentItem.Apply(model);
            }

            return Edit(contentItem, updater);
        }
    }
}
