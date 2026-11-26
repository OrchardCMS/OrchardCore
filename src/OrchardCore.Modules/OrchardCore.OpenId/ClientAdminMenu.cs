using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.OpenId;

public sealed class ClientAdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_clientRouteValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", "OrchardCore.OpenId.Client" },
    };


    internal readonly IStringLocalizer S;

    public ClientAdminMenu(IStringLocalizer<ClientAdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["OpenID Connect"], S["OpenID Connect"].PrefixPosition(), openId => openId
                    .Add(S["Authentication Client"], S["Authentication Client"].PrefixPosition(), client => client
                        .Action("Index", "Admin", s_clientRouteValues)
                        .Permission(OpenIdPermissions.ManageClientSettings)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
