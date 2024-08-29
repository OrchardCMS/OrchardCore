using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Themes;

public sealed class AdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Design"], NavigationConstants.AdminMenuDesignPosition, design => design
                .AddClass("themes").Id("themes")
                .Add(S["Themes"], S["Themes"].PrefixPosition(), themes => themes
                    .Action("Index", "Admin", "OrchardCore.Themes")
                    .Permission(Permissions.ApplyTheme)
                    .LocalNav()
                )
            );

        return Task.CompletedTask;
    }
}
