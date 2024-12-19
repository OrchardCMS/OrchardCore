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
        builder
            .Add(S["Security"], security => security
                .Add(S["Roles"], S["Roles"].PrefixPosition(), roles => roles
                    .AddClass("roles").Id("roles")
                    .Action("Index", "Admin", "OrchardCore.Roles")
                    .Permission(CommonPermissions.ManageRoles)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
