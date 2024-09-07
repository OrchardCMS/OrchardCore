using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Models;
using OrchardCore.Search.ViewModels;

namespace OrchardCore.Search.Drivers;

public sealed class SearchFormPartDisplayDriver : ContentPartDisplayDriver<SearchFormPart>
{
    public override IDisplayResult Display(SearchFormPart part, BuildPartDisplayContext context)
    {
        return View(GetDisplayShapeType(context), part)
            .Location("Detail", "Content");
    }

    public override IDisplayResult Edit(SearchFormPart part, BuildPartEditorContext context)
    {
        return Initialize<SearchPartViewModel>(GetEditorShapeType(context), viewModel =>
        {
            viewModel.Placeholder = part.Placeholder;
            viewModel.IndexName = part.IndexName;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SearchFormPart part, UpdatePartEditorContext context)
    {
        var model = new SearchPartViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        part.Placeholder = model.Placeholder;
        part.IndexName = string.IsNullOrWhiteSpace(model.IndexName) ? null : model.IndexName.Trim();

        return Edit(part, context);
    }
}
