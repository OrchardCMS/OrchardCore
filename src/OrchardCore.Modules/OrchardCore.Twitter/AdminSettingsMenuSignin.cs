using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Twitter;

public sealed class AdminSettingsMenuSignin : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminSettingsMenuSignin(IStringLocalizer<AdminSettingsMenuSignin> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Authentication"], authentication => authentication
                .Add(S["Sign in with X (Twitter)"], S["Sign in with X (Twitter)"].PrefixPosition(), twitter => twitter
                    .AddClass("twitter")
                    .Id("twitter")
                    .Action(GetRouteValues(TwitterConstants.Features.Signin))
                    .Permission(Permissions.ManageTwitterSignin)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
