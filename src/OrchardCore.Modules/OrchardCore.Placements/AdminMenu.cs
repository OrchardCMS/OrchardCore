using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Placements;

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
            .Add(S["Design"], design => design
                .Add(S["Placements"], S["Placements"].PrefixPosition(), import => import
                    .Action("Index", "Admin", "OrchardCore.Placements")
                    .Permission(Permissions.ManagePlacements)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
