using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Taxonomies.Core.Models;

namespace OrchardCore.Taxonomies.Drivers;

public sealed class TaxonomySortPartDisplayDriver : ContentPartDisplayDriver<TaxonomySortPart>
{
    public override IDisplayResult Edit(TaxonomySortPart part, BuildPartEditorContext context)
    {
        return Initialize<TaxonomySortPart>("TaxonomySort_Edit", model =>
        {
            model.Sort = part.Sort;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(TaxonomySortPart part, UpdatePartEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(part, Prefix, p => p.Sort);

        return Edit(part, context);
    }
}
