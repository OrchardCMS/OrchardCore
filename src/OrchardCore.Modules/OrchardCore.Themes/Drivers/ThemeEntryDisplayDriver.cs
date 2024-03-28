using System.Collections.Generic;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Themes.Models;

namespace OrchardCore.Themes.Drivers;

public class ThemeEntryDisplayDriver : DisplayDriver<ThemeEntry>
{
    public override IDisplayResult Display(ThemeEntry model)
    {
        var results = new List<ShapeResult>()
        {
            View("ThemeEntry_SummaryAdmin__Thumbnail", model).Location("SummaryAdmin", "Thumbnail:5"),
            View("ThemeEntry_SummaryAdmin__Title", model).Location("SummaryAdmin", "Header:5"),
            View("ThemeEntry_SummaryAdmin__Descriptions", model).Location("SummaryAdmin", "Content:5"),
            View("ThemeEntry_SummaryAdmin__Attributes", model).Location("SummaryAdmin", "Tags:5"),
        };

        if (model.IsCurrent)
        {
            results.Add(View("ThemeEntry_SummaryAdmin__Current", model).Location("SummaryAdmin", "FooterStart:5"));
        }
        else
        {
            results.AddRange([
                View("ThemeEntry_SummaryAdmin__ButtonsMakeCurrent", model).Location("SummaryAdmin", "FooterStart:5"),
                View("ThemeEntry_SummaryAdmin__ButtonsToggleState", model).Location("SummaryAdmin", "FooterEnd:5")
            ]);
        }

        return Combine(results);
    }
}
