using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing.Models;
using OrchardCore.Indexing.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Indexing.Drivers;

internal sealed class IndexEntityDisplayDriver : DisplayDriver<IndexEntity>
{
    internal readonly IStringLocalizer S;

    public IndexEntityDisplayDriver(IStringLocalizer<IndexEntityDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(IndexEntity entity, BuildDisplayContext context)
    {
        return CombineAsync(
            View("IndexEntity_Fields_SummaryAdmin", entity).Location("Content:1"),
            View("IndexEntity_Buttons_SummaryAdmin", entity).Location("Actions:5"),
            View("IndexEntity_DefaultTags_SummaryAdmin", entity).Location("Tags:5"),
            View("IndexEntity_DefaultMeta_SummaryAdmin", entity).Location("Meta:5")
        );
    }

    public override IDisplayResult Edit(IndexEntity entity, BuildEditorContext context)
    {
        return Initialize<EditIndexEntityViewModel>("IndexEntityFields_Edit", model =>
        {
            model.DisplayText = entity.DisplayText;
        }).Location("Content:1");
    }

    public override async Task<IDisplayResult> UpdateAsync(IndexEntity entity, UpdateEditorContext context)
    {
        var model = new EditIndexEntityViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (string.IsNullOrEmpty(model.DisplayText))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.DisplayText), S["The display-text is required field."]);
        }

        entity.DisplayText = model.DisplayText;

        return Edit(entity, context);
    }
}
