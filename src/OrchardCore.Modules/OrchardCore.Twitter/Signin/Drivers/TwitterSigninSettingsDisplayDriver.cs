using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Twitter.Signin.Settings;
using OrchardCore.Twitter.Signin.ViewModels;

namespace OrchardCore.Twitter.Signin.Drivers;

public sealed class TwitterSigninSettingsDisplayDriver : SiteDisplayDriver<TwitterSigninSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public TwitterSigninSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<TwitterSigninSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        S = stringLocalizer;
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
        }).Location(l => l.Zone("Content", "5").Card(S["Sign in"], "10"))
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

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
