using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Themes.Models;

namespace OrchardCore.Themes.Drivers;

public sealed class ThemeEntryDisplayDriver : DisplayDriver<ThemeEntry>
{
    public override Task<IDisplayResult> DisplayAsync(ThemeEntry model, BuildDisplayContext context)
    {
        var results = new List<ShapeResult>()
        {
            View("ThemeEntry_SummaryAdmin__Thumbnail", model).Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Thumbnail:5"),
            View("ThemeEntry_SummaryAdmin__Title", model).Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Header:5"),
            View("ThemeEntry_SummaryAdmin__Descriptions", model).Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:5"),
            View("ThemeEntry_SummaryAdmin__Attributes", model).Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
        };

        if (model.IsCurrent)
        {
            results.Add(View("ThemeEntry_SummaryAdmin__Current", model).Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "FooterStart:5"));
        }
        else
        {
            results.AddRange([
                View("ThemeEntry_SummaryAdmin__ButtonsMakeCurrent", model).Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "FooterStart:5"),
                View("ThemeEntry_SummaryAdmin__ButtonsToggleState", model).Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "FooterEnd:5")
            ]);
        }

        return CombineAsync(results);
    }
}
