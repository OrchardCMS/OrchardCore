using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Model;
using OrchardCore.Search.ViewModel;

namespace OrchardCore.Search.Drivers;

public class SearchPartDisplayDriver : ContentPartDisplayDriver<SearchPart>
{
    public override IDisplayResult Display(SearchPart part)
    {
        return View("SearchPart", part).Location("Detail", "Content");
    }

    public override IDisplayResult Edit(SearchPart part)
    {
        return Initialize<SearchPartViewModel>("SearchPart_Edit", viewModel =>
        {
            viewModel.Placeholder = part.Placeholder;
            viewModel.IndexName = part.IndexName;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SearchPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var model = new SearchPartViewModel();

        if (await updater.TryUpdateModelAsync(model, Prefix))
        {
            part.Placeholder = model.Placeholder;
            part.IndexName = String.IsNullOrWhiteSpace(model.IndexName) ? null : model.IndexName.Trim();
        }

        return Edit(part);
    }
}
