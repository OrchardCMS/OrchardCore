using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Templates;

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
                .Add(S["Templates"], S["Templates"].PrefixPosition(), import => import
                    .Action("Index", "Template", "OrchardCore.Templates")
                    .Permission(Permissions.ManageTemplates)
                    .LocalNav()
                )
            );

        return Task.CompletedTask;
    }
}
