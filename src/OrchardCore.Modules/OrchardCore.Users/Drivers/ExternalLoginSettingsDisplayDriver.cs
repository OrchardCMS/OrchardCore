using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class ExternalLoginSettingsDisplayDriver : SiteDisplayDriver<ExternalLoginSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellReleaseManager _shellReleaseManager;

    public ExternalLoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IShellReleaseManager shellReleaseManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _shellReleaseManager = shellReleaseManager;
    }

    protected override string SettingsGroupId
        => LoginSettingsDisplayDriver.GroupId;

    public override IDisplayResult Edit(ISite site, ExternalLoginSettings settings, BuildEditorContext context)
    {
        return Initialize<ExternalLoginSettings>("ExternalLoginSettings_Edit", model =>
        {
            model.UseExternalProviderIfOnlyOneDefined = settings.UseExternalProviderIfOnlyOneDefined;
            model.UseScriptToSyncProperties = settings.UseScriptToSyncProperties;
            model.SyncPropertiesScript = settings.SyncPropertiesScript;
        }).Location("Content:5#External Login;10")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.ManageUsers))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ExternalLoginSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        {
            return null;
        }

        var valueBefore = settings.UseExternalProviderIfOnlyOneDefined;

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        if (valueBefore != settings.UseExternalProviderIfOnlyOneDefined)
        {
            _shellReleaseManager.RequestRelease();
        }

        return Edit(site, settings, context);
    }
}
