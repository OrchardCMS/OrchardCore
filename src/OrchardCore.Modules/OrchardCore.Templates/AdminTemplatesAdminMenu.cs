using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Templates;

public sealed class AdminTemplatesAdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminTemplatesAdminMenu(IStringLocalizer<AdminTemplatesAdminMenu> localizer)
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
                .Add(S["Admin Templates"], S["Admin Templates"].PrefixPosition(), import => import
                    .Action("Admin", "Template", "OrchardCore.Templates")
                    .Permission(AdminTemplatesPermissions.ManageAdminTemplates)
                    .LocalNav()
                )
            );

        return Task.CompletedTask;
    }
}
