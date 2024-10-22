using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Facebook;

public sealed class AdminSettingsMenuLogin : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminSettingsMenuLogin(IStringLocalizer<AdminSettingsMenuLogin> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Authentication"], authentication => authentication
                .Add(S["Meta"], S["Meta"].PrefixPosition(), meta => meta
                    .AddClass("facebook")
                    .Id("facebook")
                    .Action(GetRouteValues(FacebookConstants.Features.Login))
                    .Permission(Permissions.ManageFacebookApp)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
