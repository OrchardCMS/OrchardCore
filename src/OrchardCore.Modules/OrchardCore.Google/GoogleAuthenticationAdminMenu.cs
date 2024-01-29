using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Google
{
    [Feature(GoogleConstants.Features.GoogleAuthentication)]
    public class GoogleAuthenticationAdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

        public GoogleAuthenticationAdminMenu(IStringLocalizer<GoogleAuthenticationAdminMenu> localizer)
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
                    .Add(S["Google"], S["Google"].PrefixPosition(), google => google
                        .AddClass("google")
                        .Id("google")
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GoogleConstants.Features.GoogleAuthentication })
                        .Permission(Permissions.ManageGoogleAuthentication)
                        .LocalNav()
                    )
                )
            );

            return Task.CompletedTask;
        }
    }

    [Feature(GoogleConstants.Features.GoogleAnalytics)]
    public class GoogleAnalyticsAdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

        public GoogleAnalyticsAdminMenu(IStringLocalizer<GoogleAnalyticsAdminMenu> localizer)
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
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(S["Google Analytics"], S["Google Analytics"].PrefixPosition(), google => google
                            .AddClass("googleAnalytics").Id("googleAnalytics")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GoogleConstants.Features.GoogleAnalytics })
                            .Permission(Permissions.ManageGoogleAnalytics)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }

    [Feature(GoogleConstants.Features.GoogleTagManager)]
    public class GoogleTagManagerAdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

        public GoogleTagManagerAdminMenu(IStringLocalizer<GoogleTagManagerAdminMenu> localizer)
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
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(S["Google Tag Manager"], S["Google Tag Manager"].PrefixPosition(), google => google
                            .AddClass("googleTagManager")
                            .Id("googleTagManager")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GoogleConstants.Features.GoogleTagManager })
                            .Permission(Permissions.ManageGoogleTagManager)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
