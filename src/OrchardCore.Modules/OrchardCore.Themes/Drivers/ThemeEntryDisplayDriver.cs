using OrchardCore.DisplayManagement;
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
            View("ThemeEntry_SummaryAdmin__Thumbnail", model).Location(DisplayType.SummaryAdmin, "Thumbnail:5"),
            View("ThemeEntry_SummaryAdmin__Title", model).Location(DisplayType.SummaryAdmin, "Header:5"),
            View("ThemeEntry_SummaryAdmin__Descriptions", model).Location(DisplayType.SummaryAdmin, "Content:5"),
            View("ThemeEntry_SummaryAdmin__Attributes", model).Location(DisplayType.SummaryAdmin, "Tags:5"),
        };

        if (model.IsCurrent)
        {
            results.Add(View("ThemeEntry_SummaryAdmin__Current", model).Location(DisplayType.SummaryAdmin, "FooterStart:5"));
        }
        else
        {
            results.AddRange([
                View("ThemeEntry_SummaryAdmin__ButtonsMakeCurrent", model).Location(DisplayType.SummaryAdmin, "FooterStart:5"),
                View("ThemeEntry_SummaryAdmin__ButtonsToggleState", model).Location(DisplayType.SummaryAdmin, "FooterEnd:5")
            ]);
        }

        return CombineAsync(results);
    }
}
