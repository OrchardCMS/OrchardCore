using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Roles;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Security"], security => security
                    .Add(S["Roles"], S["Roles"].PrefixPosition(), roles => roles
                        .AddClass("roles")
                        .Id("roles")
                        .Action("Index", "Admin", "OrchardCore.Roles")
                        .Permission(RolesPermissions.ManageRoles)
                        .LocalNav()
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Access Control"], accessControl => accessControl
                .Add(S["Roles"], S["Roles"].PrefixPosition(), roles => roles
                    .AddClass("roles")
                    .Id("roles")
                    .Action("Index", "Admin", "OrchardCore.Roles")
                    .Permission(RolesPermissions.ManageRoles)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
