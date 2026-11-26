using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;
using OrchardCore.ReverseProxy.Drivers;

namespace OrchardCore.ReverseProxy;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", ReverseProxySettingsDisplayDriver.GroupId},
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["Reverse Proxy"], S["Reverse Proxy"].PrefixPosition(), proxy => proxy
                    .AddClass("reverseproxy")
                    .Id("reverseproxy")
                    .Action("Index", "Admin", s_routeValues)
                    .Permission(Permissions.ManageReverseProxySettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
