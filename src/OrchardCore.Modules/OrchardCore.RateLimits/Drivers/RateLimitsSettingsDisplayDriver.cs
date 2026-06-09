using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Settings;
using OrchardCore.RateLimits.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.RateLimits.Drivers;

internal sealed class RateLimitsSettingsDisplayDriver : SiteDisplayDriver<RateLimitsSettings>
{
    internal const string GroupId = "RateLimits";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellReleaseManager _shellReleaseManager;

    public RateLimitsSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IShellReleaseManager shellReleaseManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _shellReleaseManager = shellReleaseManager;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, RateLimitsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, RateLimitsPermissions.ManageRateLimits))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<RateLimitsSettingsViewModel>("RateLimitsSettings_Edit", model =>
        {
            model.EnableGlobalRateLimiter = settings.EnableGlobalRateLimiter;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, RateLimitsSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, RateLimitsPermissions.ManageRateLimits))
        {
            return null;
        }

        var model = new RateLimitsSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var hasChange = settings.EnableGlobalRateLimiter != model.EnableGlobalRateLimiter;

        settings.EnableGlobalRateLimiter = model.EnableGlobalRateLimiter;

        if (hasChange)
        {
            _shellReleaseManager.RequestRelease();
        }

        return await EditAsync(site, settings, context);
    }
}
