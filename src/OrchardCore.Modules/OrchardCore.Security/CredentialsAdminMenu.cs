using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Security.Core;

namespace OrchardCore.Security;

public sealed class CredentialsAdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public CredentialsAdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Credentials"], S["Credentials"].PrefixPosition(), security => security
                    .Permission(SecurityConstants.Permissions.ManageCredentials)
                    .Action("Index", "Credentials", SecurityConstants.Features.Area)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
