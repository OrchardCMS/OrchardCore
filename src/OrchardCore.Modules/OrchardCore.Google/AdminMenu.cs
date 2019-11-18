using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Google
{
    [Feature(GoogleConstants.Features.GoogleAuthentication)]
    public class GoogleAuthenticationAdminMenu : INavigationProvider
    {
        private readonly ShellDescriptor _shellDescriptor;

        public GoogleAuthenticationAdminMenu(
            IStringLocalizer<GoogleAuthenticationAdminMenu> localizer,
            ShellDescriptor shellDescriptor)
        {
            T = localizer;
            _shellDescriptor = shellDescriptor;
        }

        public IStringLocalizer T { get; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(T["Security"], security => security
                        .Add(T["Authentication"], authentication => authentication
                        .Add(T["Google"], "16", settings => settings
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
        private readonly ShellDescriptor _shellDescriptor;

        public GoogleAnalyticsAdminMenu(
            IStringLocalizer<GoogleAnalyticsAdminMenu> localizer,
            ShellDescriptor shellDescriptor)
        {
            T = localizer;
            _shellDescriptor = shellDescriptor;
        }

        public IStringLocalizer T { get; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(T["Configuration"], configuration => configuration
                        .Add(T["Settings"], settings => settings
                            .Add(T["Google Analytics"], T["Google Analytics"], settings => settings
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

}
