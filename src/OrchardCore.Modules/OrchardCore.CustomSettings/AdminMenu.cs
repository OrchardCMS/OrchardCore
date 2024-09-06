using System.Collections.Concurrent;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.CustomSettings;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly ConcurrentDictionary<string, RouteValueDictionary> _routeValues = [];

    private readonly CustomSettingsService _customSettingsService;

    internal readonly IStringLocalizer S;

    public AdminMenu(
        IStringLocalizer<AdminMenu> localizer,
        CustomSettingsService customSettingsService)
    {
        S = localizer;
        _customSettingsService = customSettingsService;
    }

    protected override async ValueTask BuildAsync(NavigationBuilder builder)
    {
        foreach (var type in await _customSettingsService.GetAllSettingsTypesAsync())
        {
            if (!_routeValues.TryGetValue(type.Name, out var routeValues))
            {
                routeValues = new RouteValueDictionary()
                {
                     { "area", "OrchardCore.Settings" },
                     { "groupId", type.Name },
                };

                _routeValues[type.Name] = routeValues;
            }

            var htmlName = type.Name.HtmlClassify();

            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(new LocalizedString(type.DisplayName, type.DisplayName), type.DisplayName.PrefixPosition(), layers => layers
                            .Action("Index", "Admin", routeValues)
                            .AddClass(htmlName)
                            .Id(htmlName)
                            .Permission(Permissions.CreatePermissionForType(type))
                            .Resource(type.Name)
                            .LocalNav()
                        )
                    )
                );
        }
    }
}
