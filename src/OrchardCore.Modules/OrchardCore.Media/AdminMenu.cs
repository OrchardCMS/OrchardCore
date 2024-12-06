using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Content"], content => content
                .AddClass("media")
                .Id("media")
                .Add(S["Media library"], S["Media library"].PrefixPosition(), media => media
                    .Permission(Permissions.ManageMedia)
                    .Action("Index", "Admin", "OrchardCore.Media")
                    .LocalNav()
                )
            );

        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                        .Add(S["Media options"], S["Media options"].PrefixPosition(), options => options
                            .Action("Options", "Admin", "OrchardCore.Media")
                            .Permission(Permissions.ViewMediaOptions)
                            .LocalNav()
                        )
                        .Add(S["Media profiles"], S["Media profiles"].PrefixPosition(), mediaProfiles => mediaProfiles
                            .Action("Index", "MediaProfiles", "OrchardCore.Media")
                            .Permission(Permissions.ManageMediaProfiles)
                            .LocalNav()
                        )
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                    .Add(S["Media options"], S["Media options"].PrefixPosition(), options => options
                        .Action("Options", "Admin", "OrchardCore.Media")
                        .Permission(Permissions.ViewMediaOptions)
                        .LocalNav()
                    )
                    .Add(S["Media profiles"], S["Media profiles"].PrefixPosition(), mediaProfiles => mediaProfiles
                        .Action("Index", "MediaProfiles", "OrchardCore.Media")
                        .Permission(Permissions.ManageMediaProfiles)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
