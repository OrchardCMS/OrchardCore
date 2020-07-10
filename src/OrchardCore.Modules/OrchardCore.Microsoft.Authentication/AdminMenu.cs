using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Microsoft.Authentication
{
    [Feature(MicrosoftAuthenticationConstants.Features.MicrosoftAccount)]
    public class AdminMenuMicrosoftAccount : INavigationProvider
    {
        private readonly IStringLocalizer S;

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
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IStringLocalizer S;

        public AdminMenuAAD(
            IStringLocalizer<AdminMenuAAD> localizer,
            ShellDescriptor shellDescriptor)
        {
            S = localizer;
            _shellDescriptor = shellDescriptor;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Security"], security => security
                        .Add(S["Authentication"], authentication => authentication
                        .Add(S["Azure Active Directory"], S["Azure Active Directory"].PrefixPosition(), client => client
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = MicrosoftAuthenticationConstants.Features.AAD })
                            .Permission(Permissions.ManageMicrosoftAuthentication)
                            .LocalNav())
                    ));
            }
            return Task.CompletedTask;
        }
    }
}
