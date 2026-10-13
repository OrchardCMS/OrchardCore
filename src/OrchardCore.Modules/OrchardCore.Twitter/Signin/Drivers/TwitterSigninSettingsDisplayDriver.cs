using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Options;
using OrchardCore.Settings;
using OrchardCore.Twitter.Signin.Settings;
using OrchardCore.Twitter.Signin.ViewModels;

namespace OrchardCore.Twitter.Signin.Drivers;

public sealed class TwitterSigninSettingsDisplayDriver : SiteDisplayDriver<TwitterSigninSettings>
{
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TwitterSigninSettingsDisplayDriver(
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _optionsUpdateNotifier = optionsUpdateNotifier;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override string SettingsGroupId
        => TwitterConstants.Features.Signin;

    public override async Task<IDisplayResult> EditAsync(ISite site, TwitterSigninSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterSignin))
        {
            return null;
        }

        return Initialize<TwitterSigninSettingsViewModel>("TwitterSigninSettings_Edit", model =>
        {
            if (settings.CallbackPath.HasValue)
            {
                model.CallbackPath = settings.CallbackPath;
            }
            model.SaveTokens = settings.SaveTokens;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }
    public override async Task<IDisplayResult> UpdateAsync(ISite site, TwitterSigninSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterSignin))
        {
            return null;
        }

        var model = new TwitterSigninSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.CallbackPath = model.CallbackPath;
        settings.SaveTokens = model.SaveTokens;

        // Refresh the options built from these settings, so that they apply without restarting the tenant.
        _optionsUpdateNotifier.RequestTwitterSigninUpdate();

        return await EditAsync(site, settings, context);
    }
}
