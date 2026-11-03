using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using LiquidPermissions = OrchardCore.Liquid.Permissions;

namespace OrchardCore.Templates;

public sealed class AdminTemplatesAdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminTemplatesAdminMenu(IStringLocalizer<AdminTemplatesAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Design"], design => design
                .Add(S["Admin Templates"], S["Admin Templates"].PrefixPosition(), import => import
                    .Action("Admin", "Template", "OrchardCore.Templates")
                    .Permission(AdminTemplatesPermissions.ManageAdminTemplates)
                    .Permission(LiquidPermissions.ManageLiquidTemplates)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
