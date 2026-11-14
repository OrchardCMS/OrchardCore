using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Options;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Drivers;

public sealed class DebugSettingsDisplayDriver : SiteDisplayDriver<DebugSettings>
{
    public const string GroupId = "debugging";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => GroupId;

    public DebugSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IStringLocalizer<DebugSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _optionsUpdateNotifier = optionsUpdateNotifier;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(ISite site, DebugSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(
            _httpContextAccessor.HttpContext?.User,
            SettingsPermissions.ManageDebuggingSettings))
        {
            return null;
        }

        return Initialize<DebugSettingsViewModel>("DebugSettings_Edit", model =>
        {
            model.WriteShapeDebugInformation = settings.WriteShapeDebugInformation;
        }).Location(l => l.Zone("Content", "5").Card(S["Shapes"], "10"))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, DebugSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(
            _httpContextAccessor.HttpContext?.User,
            SettingsPermissions.ManageDebuggingSettings))
        {
            return null;
        }

        var model = new DebugSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (settings.WriteShapeDebugInformation != model.WriteShapeDebugInformation)
        {
            _optionsUpdateNotifier.RequestUpdate<ShapeRenderingOptions>();
        }

        settings.WriteShapeDebugInformation = model.WriteShapeDebugInformation;

        return await EditAsync(site, settings, context);
    }
}
