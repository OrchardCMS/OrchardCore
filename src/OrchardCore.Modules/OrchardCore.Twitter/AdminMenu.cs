using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Twitter
{
    [Feature(TwitterConstants.Features.Twitter)]
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Security"], security => security
                        .Add(S["Authentication"], authentication => authentication
                        .Add(S["Twitter"], S["Twitter"].PrefixPosition(), settings => settings
                        .AddClass("twitter").Id("twitter")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = TwitterConstants.Features.Twitter })
                            .Permission(Permissions.ManageTwitter)
                            .LocalNav())
                    ));
            }
            return Task.CompletedTask;
        }
    }

    [Feature(TwitterConstants.Features.Signin)]
    public class AdminMenuSignin : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenuSignin(IStringLocalizer<AdminMenuSignin> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Security"], security => security
                        .Add(S["Twitter"], S["Twitter"].PrefixPosition(), settings => settings
                        .AddClass("twitter").Id("twitter")
                        .Add(S["Sign in with Twitter"], S["Sign in with Twitter"].PrefixPosition(), client => client
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = TwitterConstants.Features.Signin })
                            .Permission(Permissions.ManageTwitterSignin)
                            .LocalNav())
                    ));
            }
            return Task.CompletedTask;
        }
    }
}
