using System.Threading.Tasks;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.ViewModels;

namespace OrchardCore.Lists.Drivers
{
    public class ListPartContentsAdminListDisplayDriver : DisplayDriver<ContentOptionsViewModel>
    {
        protected override void BuildPrefix(ContentOptionsViewModel model, string htmlFieldPrefix)
        {
            Prefix = "ListPart";
        }

        public override Task<IDisplayResult> EditAsync(ContentOptionsViewModel model, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Dynamic("ContentsAdminList__ListPartFilter").Location("Actions:20")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentOptionsViewModel model, UpdateEditorContext context)
        {
            var viewModel = new ListPartContentsAdminFilterViewModel();
            await context.Updater.TryUpdateModelAsync(viewModel, nameof(ListPart));

            if (viewModel.ShowListContentTypes)
            {
                model.RouteValues.TryAdd("ListPart.ShowListContentTypes", viewModel.ShowListContentTypes);
            }

            if (!string.IsNullOrEmpty(viewModel.ListContentItemId))
            {
                model.RouteValues.TryAdd("ListPart.ListContentItemId", viewModel.ListContentItemId);
            }

            return await EditAsync(model, context);
        }
    }
}
