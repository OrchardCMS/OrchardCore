using Microsoft.AspNetCore.Authorization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Google.Analytics.Settings;
using OrchardCore.Google.Analytics.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Google.Analytics.Drivers;

public sealed class GoogleAnalyticsSettingsDisplayDriver : SiteDisplayDriver<GoogleAnalyticsSettings>
{
    protected override string SettingsGroupId
        => GoogleConstants.Features.GoogleAnalytics;

    private readonly IAuthorizationService _authorizationService;

    public GoogleAnalyticsSettingsDisplayDriver(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public override async Task<IDisplayResult> EditAsync(ISite site, GoogleAnalyticsSettings settings, BuildEditorContext context)
    {
        var user = context.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleAnalytics))
        {
            return null;
        }

        return Initialize<GoogleAnalyticsSettingsViewModel>("GoogleAnalyticsSettings_Edit", model =>
        {
            model.TrackingID = settings.TrackingID;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, GoogleAnalyticsSettings settings, UpdateEditorContext context)
    {
        var user = context.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleAnalytics))
        {
            return null;
        }

        var model = new GoogleAnalyticsSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (context.Updater.ModelState.IsValid)
        {
            settings.TrackingID = model.TrackingID;
        }

        return await EditAsync(site, settings, context);
    }
}
