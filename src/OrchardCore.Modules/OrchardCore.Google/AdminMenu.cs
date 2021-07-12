using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Google
{
    [Feature(GoogleConstants.Features.GoogleAuthentication)]
    public class GoogleAuthenticationAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public GoogleAuthenticationAdminMenu(IStringLocalizer<GoogleAuthenticationAdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Security"], security => security
                        .Add(S["Authentication"], authentication => authentication
                        .Add(S["Google"], S["Google"].PrefixPosition(), settings => settings
                        .AddClass("google").Id("google")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GoogleConstants.Features.GoogleAuthentication })
                            .Permission(Permissions.ManageGoogleAuthentication)
                            .LocalNav())
                    ));
            }
            return Task.CompletedTask;
        }
    }

    [Feature(GoogleConstants.Features.GoogleAnalytics)]
    public class GoogleAnalyticsAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public GoogleAnalyticsAdminMenu(IStringLocalizer<GoogleAnalyticsAdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Configuration"], configuration => configuration
                        .Add(S["Settings"], settings => settings
                            .Add(S["Google Analytics"], S["Google Analytics"].PrefixPosition(), settings => settings
                            .AddClass("googleAnalytics").Id("googleAnalytics")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GoogleConstants.Features.GoogleAnalytics })
                                .Permission(Permissions.ManageGoogleAnalytics)
                                .LocalNav())
                            )
                        );
            }
            return Task.CompletedTask;
        }
    }

    [Feature(GoogleConstants.Features.GoogleTagManager)]
    public class GoogleTagManagerAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public GoogleTagManagerAdminMenu(IStringLocalizer<GoogleTagManagerAdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Configuration"], configuration => configuration
                        .Add(S["Settings"], settings => settings
                            .Add(S["Google Tag Manager"], S["Google Tag Manager"].PrefixPosition(), settings => settings
                            .AddClass("googleTagManager").Id("googleTagManager")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GoogleConstants.Features.GoogleTagManager })
                                .Permission(Permissions.ManageGoogleTagManager)
                                .LocalNav())
                            )
                        );
            }
            return Task.CompletedTask;
        }
    }
}
