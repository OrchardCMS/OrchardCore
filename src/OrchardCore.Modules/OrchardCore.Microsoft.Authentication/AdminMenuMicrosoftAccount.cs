using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Microsoft.Authentication
{
    [Feature(MicrosoftAuthenticationConstants.Features.MicrosoftAccount)]
    public class AdminMenuMicrosoftAccount : INavigationProvider
    {
        private static readonly RouteValueDictionary _routeValues = new()
        {
            { "area", "OrchardCore.Settings" },
            { "groupId", MicrosoftAuthenticationConstants.Features.MicrosoftAccount },
        };

        protected readonly IStringLocalizer S;

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

    [Feature(MicrosoftAuthenticationConstants.Features.AAD)]
    public class AdminMenuAAD : INavigationProvider
    {
        private static readonly RouteValueDictionary _routeValues = new()
        {
            { "area", "OrchardCore.Settings" },
            { "groupId", MicrosoftAuthenticationConstants.Features.AAD },
        };

        protected readonly IStringLocalizer S;

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
}
