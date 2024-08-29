using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Shortcodes;

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
            .Add(S["Design"], design => design
                .Add(S["Shortcodes"], S["Shortcodes"].PrefixPosition(), import => import
                    .Action("Index", "Admin", "OrchardCore.Shortcodes")
                    .Permission(Permissions.ManageShortcodeTemplates)
                    .LocalNav()
                )
            );

        return Task.CompletedTask;
    }
}
