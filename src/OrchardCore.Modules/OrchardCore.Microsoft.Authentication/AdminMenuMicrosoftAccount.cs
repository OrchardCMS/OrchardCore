using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Microsoft.Authentication;

public sealed class AdminMenuMicrosoftAccount : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", MicrosoftAuthenticationConstants.Features.MicrosoftAccount },
    };

    internal readonly IStringLocalizer S;

    public AdminMenuMicrosoftAccount(IStringLocalizer<AdminMenuMicrosoftAccount> localizer)
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
            .Add(S["Security"], security => security
                .Add(S["Authentication"], authentication => authentication
                    .Add(S["Microsoft"], S["Microsoft"].PrefixPosition(), microsoft => microsoft
                        .AddClass("microsoft")
                        .Id("microsoft")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageMicrosoftAuthentication)
                        .LocalNav()
                    )
                )
           );

        return Task.CompletedTask;
    }
}

public sealed class AdminMenuAAD : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", MicrosoftAuthenticationConstants.Features.AAD },
    };

    internal readonly IStringLocalizer S;

    public AdminMenuAAD(IStringLocalizer<AdminMenuAAD> localizer) => S = localizer;

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Security"], security => security
                .Add(S["Authentication"], authentication => authentication
                    .Add(S["Microsoft Entra ID"], S["Microsoft Entra ID"].PrefixPosition(), client => client
                        .AddClass("microsoft-entra-id").Id("microsoft-entra-id")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageMicrosoftAuthentication)
                        .LocalNav())
            ));

        return Task.CompletedTask;
    }
}
