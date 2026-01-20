using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class ActivityMetadataDisplayDriver : SectionDisplayDriver<IActivity, ActivityMetadata>
    {
        public override IDisplayResult Edit(ActivityMetadata section, BuildEditorContext context)
        {
            return Initialize<ActivityMetadataEditViewModel>("ActivityMetadata_Edit", viewModel =>
            {
                viewModel.Title = section.Title;
            }).Location("Content:before");
        }

        public override async Task<IDisplayResult> UpdateAsync(ActivityMetadata section, IUpdateModel updater, BuildEditorContext context)
        {
            var viewModel = new ActivityMetadataEditViewModel();

            if (await context.Updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                section.Title = viewModel.Title?.Trim();
            }

            return await EditAsync(section, context);
        }
    }
}
