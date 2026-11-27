using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Media"], "after.15", media => media
                .AddClass("media")
                .Id("media")
                .Add(S["Library"], S["Library"].PrefixPosition("1"), library => library
                    .Permission(MediaPermissions.ManageMedia)
                    .Action("Index", "Admin", "OrchardCore.Media")
                    .LocalNav()
                )
                .Add(S["Profiles"], S["Profiles"].PrefixPosition("5"), mediaProfiles => mediaProfiles
                    .Action("Index", "MediaProfiles", "OrchardCore.Media")
                    .Permission(MediaPermissions.ManageMediaProfiles)
                    .LocalNav()
                )
            );

        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                    .Add(S["Options"], S["Options"].PrefixPosition(), options => options
                        .Action("Options", "Admin", "OrchardCore.Media")
                        .Permission(MediaPermissions.ViewMediaOptions)
                        .LocalNav()
                    )
                    .Add(S["API"], S["API"].PrefixPosition(), api => api
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = MediaApiSettings.GroupId })
                        .Permission(MediaPermissions.ManageMediaApiSettings)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
