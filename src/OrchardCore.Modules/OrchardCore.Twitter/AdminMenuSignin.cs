using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Twitter;

public sealed class AdminMenuSignIn : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", TwitterConstants.Features.Signin },
    };

    internal readonly IStringLocalizer S;

    public AdminMenuSignIn(IStringLocalizer<AdminMenuSignIn> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
           .Add(S["Security"], security => security
               .Add(S["Authentication"], authentication => authentication

               .Add(S["Sign In with X (Twitter)"], S["Sign In with X (Twitter)"].PrefixPosition(), twitter => twitter
                   .AddClass("twitter")
                   .Id("twitter")
                   .Action("Index", "Admin", _routeValues)
                   .Permission(Permissions.ManageTwitterSignin)
                   .LocalNav())
               )
           );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Security"], S["Security"].PrefixPosition(), security => security
                    .Add(S["Authentication"], S["Authentication"].PrefixPosition(), authentication => authentication
                        .Add(S["Sign In with X (Twitter)"], S["Sign In with X (Twitter)"].PrefixPosition(), twitter => twitter
                            .AddClass("twitter")
                            .Id("twitter")
                            .Action("Index", "Admin", _routeValues)
                            .Permission(Permissions.ManageTwitterSignin)
                            .LocalNav()
                        )
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
