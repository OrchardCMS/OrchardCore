using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class BranchTaskDisplay : ActivityDisplayDriver<BranchTask>
    {
        public override IDisplayResult Display(BranchTask activity)
        {
            return Combine(
                Shape("BranchTask_Fields_Thumbnail", activity).Location("Thumbnail", "Content"),
                Shape("BranchTask_Fields_Design", activity).Location("Design", "Content")
            );
        }

        public override IDisplayResult Edit(BranchTask activity)
        {
            return Shape<BranchTaskViewModel>("BranchTask_Fields_Edit", model =>
            {
                model.Branches = string.Join(", ", activity.Branches);
            }).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(BranchTask activity, IUpdateModel updater)
        {
            var viewModel = new BranchTaskViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                activity.Branches = viewModel.Branches.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            return Edit(activity);
        }
    }
}
