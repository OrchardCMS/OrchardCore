using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Microsoft.Authentication
{
    [Feature(MicrosoftAuthenticationConstants.Features.MicrosoftAccount)]
    public class AdminMenuMicrosoftAccount : INavigationProvider
    {
        protected readonly IStringLocalizer S;

        public AdminMenuMicrosoftAccount(IStringLocalizer<AdminMenuMicrosoftAccount> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Security"], security => security
                        .Add(S["Authentication"], authentication => authentication
                        .Add(S["Microsoft"], S["Microsoft"].PrefixPosition(), client => client
                        .AddClass("microsoft").Id("microsoft")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = MicrosoftAuthenticationConstants.Features.MicrosoftAccount })
                            .Permission(Permissions.ManageMicrosoftAuthentication)
                            .LocalNav())
                    ));
            }
            return Task.CompletedTask;
        }
    }

    [Feature(MicrosoftAuthenticationConstants.Features.AAD)]
    public class AdminMenuAAD : INavigationProvider
    {
        protected readonly IStringLocalizer S;

        public AdminMenuAAD(IStringLocalizer<AdminMenuAAD> localizer) => S = localizer;

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Security"], security => security
                        .Add(S["Authentication"], authentication => authentication
                            .Add(S["Microsoft Entra ID"], S["Microsoft Entra ID"].PrefixPosition(), client => client
                                .AddClass("microsoft-entra-id").Id("microsoft-entra-id")
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = MicrosoftAuthenticationConstants.Features.AAD })
                                .Permission(Permissions.ManageMicrosoftAuthentication)
                                .LocalNav())
                    ));
            }

            return Task.CompletedTask;
        }
    }
}
