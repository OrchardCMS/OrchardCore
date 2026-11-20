using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class SmsAuthenticatorLoginSettingsDisplayDriver : SiteDisplayDriver<SmsAuthenticatorLoginSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;

    internal readonly IStringLocalizer S;

    public SmsAuthenticatorLoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        ILiquidTemplateManager liquidTemplateManager,
        IStringLocalizer<SmsAuthenticatorLoginSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _liquidTemplateManager = liquidTemplateManager;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => LoginSettingsDisplayDriver.GroupId;

    public override IDisplayResult Edit(ISite site, SmsAuthenticatorLoginSettings settings, BuildEditorContext c)
    {
        return Initialize<SmsAuthenticatorLoginSettings>("SmsAuthenticatorLoginSettings_Edit", model =>
        {
            model.Body = string.IsNullOrWhiteSpace(settings.Body)
            ? EmailAuthenticatorLoginSettings.DefaultBody
            : settings.Body;
        }).Location(l => l.Zone("Content", "15").Tab("Two-Factor Authentication").Card(S["SMS authenticator"], "50"))
        .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers), this)
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, SmsAuthenticatorLoginSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        if (!_liquidTemplateManager.Validate(settings.Body, out var bodyErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(settings.Body), string.Join(' ', bodyErrors));
        }

        return Edit(site, settings, context);
    }
}
