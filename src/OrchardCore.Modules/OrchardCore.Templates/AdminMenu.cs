using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Templates;

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
                .Add(S["Templates"], S["Templates"].PrefixPosition(), import => import
                    .Action("Index", "Template", "OrchardCore.Templates")
                    .Permission(Permissions.ManageTemplates)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
