using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Twitter;

[Feature(TwitterConstants.Features.TwitterAuthentication)]
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
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = TwitterConstants.Features.TwitterAuthentication })
                        .Permission(Permissions.ManageTwitterAuthentication)
                        .LocalNav())
                ));
        }

        return Task.CompletedTask;
    }
}
