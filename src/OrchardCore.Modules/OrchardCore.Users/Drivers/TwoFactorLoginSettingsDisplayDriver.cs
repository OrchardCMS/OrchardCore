using OrchardCore.Users.Services.Management;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class TwoFactorLoginSettingsDisplayDriver : SiteDisplayDriver<TwoFactorLoginSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => LoginSettingsDisplayDriver.GroupId;

    public TwoFactorLoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<TwoFactorLoginSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, TwoFactorLoginSettings settings, BuildEditorContext c)
    {
        return Initialize<TwoFactorLoginSettings>("TwoFactorLoginSettings_Edit", model =>
        {
            model.NumberOfRecoveryCodesToGenerate = settings.NumberOfRecoveryCodesToGenerate;
            model.RequireTwoFactorAuthentication = settings.RequireTwoFactorAuthentication;
            model.AllowRememberClientTwoFactorAuthentication = settings.AllowRememberClientTwoFactorAuthentication;
            model.UseSiteTheme = settings.UseSiteTheme;
        }).Location("Content:5#Two-Factor Authentication")
        .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers), this)
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, TwoFactorLoginSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers))
        {
            return null;
        }

        var model = UserPolicySettingsEditor.Clone(settings);
        await context.Updater.TryUpdateModelAsync(model, Prefix);
        if (model.NumberOfRecoveryCodesToGenerate < 1)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.NumberOfRecoveryCodesToGenerate), S["Number of Recovery Codes to Generate should be greater than 0."]);
        }
        if (context.Updater.ModelState.IsValid)
        {
            UserPolicySettingsEditor.Apply(settings, model);
        }

        return Edit(site, settings, context);
    }
}
