using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Templates;

public sealed class AdminTemplatesAdminMenu : AdminMenuNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminTemplatesAdminMenu(IStringLocalizer<AdminTemplatesAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override void Build(NavigationBuilder builder)
    {
        builder
            .Add(S["Design"], design => design
                .Add(S["Admin Templates"], S["Admin Templates"].PrefixPosition(), import => import
                    .Action("Admin", "Template", "OrchardCore.Templates")
                    .Permission(AdminTemplatesPermissions.ManageAdminTemplates)
                    .LocalNav()
                )
            );
    }
}
