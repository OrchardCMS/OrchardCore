using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Themes;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Design"], NavigationConstants.AdminMenuDesignPosition, design => design
                .AddClass("themes").Id("themes")
                .Add(S["Themes"], S["Themes"].PrefixPosition(), themes => themes
                    .Action("Index", "Admin", "OrchardCore.Themes")
                    .Permission(Permissions.ApplyTheme)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
