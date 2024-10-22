using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Users.Models;

namespace OrchardCore.Users;

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
            .Add(S["Security"], NavigationConstants.AdminMenuSecurityPosition, security => security
                .AddClass("security")
                .Id("security")
                .Add(S["Users"], S["Users"].PrefixPosition(), users => users
                    .AddClass("users")
                    .Id("users")
                    .Action("Index", "Admin", UserConstants.Features.Users)
                    .Permission(CommonPermissions.ListUsers)
                    .Resource(new User())
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
