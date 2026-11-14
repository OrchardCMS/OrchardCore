using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
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

    internal readonly IStringLocalizer S;

    public ExternalLoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IStringLocalizer<ExternalLoginSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _optionsUpdateNotifier = optionsUpdateNotifier;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => LoginSettingsDisplayDriver.GroupId;

    public override IDisplayResult Edit(ISite site, ExternalLoginSettings settings, BuildEditorContext context)
    {
        return Combine(
            Initialize<ExternalLoginSettings>("ExternalLoginSettings_Edit", model => PopulateViewModel(settings, model))
                .Location(l => l.Zone("Content", "5").Tab("External Login", "10").Card(S["Login"], "10"))
                .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext.User, UsersPermissions.ManageUsers), this)
                .OnGroup(SettingsGroupId),
            Initialize<ExternalLoginSettings>("ExternalLoginSettings_Edit__Script", model => PopulateViewModel(settings, model))
                .Location(l => l.Zone("Content", "5.1").Tab("External Login", "10").Card(S["User synchronization"], "20"))
                .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext.User, UsersPermissions.ManageUsers), this)
                .OnGroup(SettingsGroupId)
        );
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

    private static void PopulateViewModel(ExternalLoginSettings settings, ExternalLoginSettings model)
    {
        model.UseExternalProviderIfOnlyOneDefined = settings.UseExternalProviderIfOnlyOneDefined;
        model.UseScriptToSyncProperties = settings.UseScriptToSyncProperties;
        model.SyncPropertiesScript = settings.SyncPropertiesScript;
    }
}
