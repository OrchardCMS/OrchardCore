using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

public sealed class ActivityMetadataDisplayDriver : SectionDisplayDriver<IActivity, ActivityMetadata>
{
    public override IDisplayResult Edit(IActivity activity, ActivityMetadata section, BuildEditorContext context)
    {
        return Initialize<ActivityMetadataEditViewModel>("ActivityMetadata_Edit", viewModel =>
        {
            viewModel.Title = section.Title;
        }).Location("Content:before");
    }

    public override async Task<IDisplayResult> UpdateAsync(IActivity activity, ActivityMetadata section, UpdateEditorContext context)
    {
        var viewModel = new ActivityMetadataEditViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        section.Title = viewModel.Title?.Trim();

        return await EditAsync(activity, section, context);
    }
}
