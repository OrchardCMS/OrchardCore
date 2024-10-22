using System.Collections.Concurrent;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.CustomSettings;

public sealed class AdminSettingsMenu : SettingsNavigationProvider
{
    private static readonly ConcurrentDictionary<string, RouteValueDictionary> _routeValues = [];

    private readonly CustomSettingsService _customSettingsService;

    internal readonly IStringLocalizer S;

    public AdminSettingsMenu(
        IStringLocalizer<AdminSettingsMenu> stringLocalizer,
        CustomSettingsService customSettingsService)
    {
        S = stringLocalizer;
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
                .Add(S["Custom"], configuration => configuration
                    .Add(new LocalizedString(type.DisplayName, type.DisplayName), type.DisplayName.PrefixPosition(), layers => layers
                        .Action("Index", "Admin", routeValues)
                        .AddClass(htmlName)
                        .Id(htmlName)
                        .Permission(Permissions.CreatePermissionForType(type))
                        .Resource(type.Name)
                        .LocalNav()
                    )
                );
        }
    }
}
