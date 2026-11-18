using OrchardCore.Admin;
using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Themes.Schemes.ViewModels;

public class ThemeSchemesIndexViewModel
{
    public string ThemeId { get; set; }

    public string ThemeName { get; set; }

    public AdminListContext<ThemeSchemeEntry> List { get; set; }
}
