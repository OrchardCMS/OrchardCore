using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Layers.Drivers;

public sealed class LayerSiteSettingsDisplayDriver : SiteDisplayDriver<LayerSettings>
{
    public const string GroupId = "zones";

    private static readonly char[] _separator = [' ', ','];

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public LayerSiteSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, LayerSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageLayers))
        {
            return null;
        }

        return Initialize<LayerSettingsViewModel>("LayerSettings_Edit", model =>
        {
            model.Zones = string.Join(", ", settings.Zones);
        }).Location("Content:3")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, LayerSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageLayers))
        {
            return null;
        }

        var model = new LayerSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.Zones = (model.Zones ?? string.Empty).Split(_separator, StringSplitOptions.RemoveEmptyEntries);

        return await EditAsync(site, settings, context);
    }
}
