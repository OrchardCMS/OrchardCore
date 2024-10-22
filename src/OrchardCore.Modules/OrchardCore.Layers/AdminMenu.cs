using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Layers;

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
                .Add(S["Widgets"], S["Widgets"].PrefixPosition(), widgets => widgets
                    .Permission(Permissions.ManageLayers)
                    .Action("Index", "Admin", "OrchardCore.Layers")
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
