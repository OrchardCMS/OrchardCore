using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Utilities;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Themes.Schemes.Drivers;

/// <summary>
/// The rows of the list of the schemes of a theme, in both admin list layouts. The swatch of a scheme has an
/// alternate per theme (<c>ThemeScheme_Swatch_SummaryAdmin__{ThemeId}</c>), so a theme can draw it with its own
/// stylesheet.
/// </summary>
internal sealed class ThemeSchemeDisplayDriver : DisplayDriver<ThemeSchemeEntry>
{
    public override IDisplayResult Display(ThemeSchemeEntry entry, BuildDisplayContext context)
    {
        if (context.DisplayType is not (OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid))
        {
            return null;
        }

        if (context.Shape is Shape row)
        {
            row.Classes.Add("item");
            row.Attributes["data-filter-value"] = entry.Scheme.Name?.ToLowerInvariant() ?? string.Empty;
        }

        return Combine(
            View("ThemeScheme_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("ThemeScheme_Swatch_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Colors:5")
                .Displaying(displaying => displaying.Shape.Metadata.Alternates.Add("ThemeScheme_Swatch_SummaryAdmin__" + entry.ThemeId.EncodeAlternateElement())),
            View("ThemeScheme_Buttons_SummaryAdmin", entry).Location("Actions:5"),
            View("ThemeScheme_ActionsMenu_SummaryAdmin", entry).Location("ActionsMenu:10"));
    }
}
