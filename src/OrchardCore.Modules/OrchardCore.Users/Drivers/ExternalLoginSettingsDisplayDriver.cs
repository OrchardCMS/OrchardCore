using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Options;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class ExternalLoginSettingsDisplayDriver : SiteDisplayDriver<ExternalLoginSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;

    public ExternalLoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptionsUpdateNotifier optionsUpdateNotifier)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _optionsUpdateNotifier = optionsUpdateNotifier;
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
        .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext.User, UsersPermissions.ManageUsers), this)
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ExternalLoginSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers))
        {
            return null;
        }

        var valueBefore = settings.UseExternalProviderIfOnlyOneDefined;

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        if (valueBefore != settings.UseExternalProviderIfOnlyOneDefined)
        {
            _optionsUpdateNotifier.RequestUpdate<ExternalLoginOptions>();
        }

        return Edit(site, settings, context);
    }
}
