using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Twitter;

public sealed class AdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminSettingsMenu(IStringLocalizer<AdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["General"], general => general
                .Add(S["X (Twitter)"], S["X (Twitter)"].PrefixPosition(), twitter => twitter
                    .AddClass("twitter")
                    .Id("twitter")
                    .Action(GetRouteValues(TwitterConstants.Features.Twitter))
                    .Permission(Permissions.ManageTwitter)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
