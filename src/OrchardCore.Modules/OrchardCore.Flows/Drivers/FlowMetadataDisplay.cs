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
        public override IDisplayResult Edit(ContentItem model, IUpdateModel updater)
        {
            var flowMetadata = model.As<FlowMetadata>();

            if (flowMetadata == null)
            {
                return null;
            }

            return Initialize<FlowMetadataEditViewModel>("FlowMetadata_Edit", m =>
            {
                m.Alignment = flowMetadata.Alignment;
                m.Size = flowMetadata.Size;
                m.Classes = flowMetadata.Classes != null ? String.Join(" ", flowMetadata.Classes) : null;
            }).Location("Footer");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItem contentItem, IUpdateModel updater)
        {
            var flowMetadata = contentItem.As<FlowMetadata>();

            if (flowMetadata == null)
            {
                return null;
            }

            var viewModel = new FlowMetadataEditViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                contentItem.Alter<FlowMetadata>(model => {
                    model.Alignment = viewModel.Alignment;
                    model.Size = viewModel.Size;
                    model.Classes = !String.IsNullOrEmpty(viewModel.Classes) ? viewModel.Classes.Split(" ", StringSplitOptions.RemoveEmptyEntries) : null;
                });
            }

            return Edit(contentItem, updater);
        }
    }
}
