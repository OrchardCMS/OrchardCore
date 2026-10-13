using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Options;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Facebook.Login.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Login.Drivers;

public sealed class FacebookLoginSettingsDisplayDriver : SiteDisplayDriver<FacebookLoginSettings>
{
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FacebookLoginSettingsDisplayDriver(
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _optionsUpdateNotifier = optionsUpdateNotifier;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }
    protected override string SettingsGroupId
        => FacebookConstants.Features.Login;

    public override async Task<IDisplayResult> EditAsync(ISite site, FacebookLoginSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
        {
            return null;
        }

        return Initialize<FacebookLoginSettingsViewModel>("FacebookLoginSettings_Edit", model =>
        {
            model.CallbackPath = settings.CallbackPath.Value;
            model.SaveTokens = settings.SaveTokens;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, FacebookLoginSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
        {
            return null;
        }

        var model = new FacebookLoginSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.CallbackPath = model.CallbackPath;
        settings.SaveTokens = model.SaveTokens;

        // Refresh the options built from these settings, so that they apply without restarting the tenant.
        _optionsUpdateNotifier.RequestFacebookLoginUpdate();

        return await EditAsync(site, settings, context);
    }
}
