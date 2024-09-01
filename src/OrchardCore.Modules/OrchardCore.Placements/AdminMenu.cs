using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Placements;

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
                .Add(S["Placements"], S["Placements"].PrefixPosition(), import => import
                    .Action("Index", "Admin", "OrchardCore.Placements")
                    .Permission(Permissions.ManagePlacements)
                    .LocalNav()
                )
            );

        return Task.CompletedTask;
    }
}
