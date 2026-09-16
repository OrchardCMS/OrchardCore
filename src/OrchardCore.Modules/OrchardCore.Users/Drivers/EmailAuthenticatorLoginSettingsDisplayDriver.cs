using OrchardCore.Users.Services.Management;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class EmailAuthenticatorLoginSettingsDisplayDriver : SiteDisplayDriver<EmailAuthenticatorLoginSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;

    public EmailAuthenticatorLoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        ILiquidTemplateManager liquidTemplateManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _liquidTemplateManager = liquidTemplateManager;
    }

    protected override string SettingsGroupId
        => LoginSettingsDisplayDriver.GroupId;

    public override IDisplayResult Edit(ISite site, EmailAuthenticatorLoginSettings settings, BuildEditorContext context)
    {
        return Initialize<EmailAuthenticatorLoginSettings>("EmailAuthenticatorLoginSettings_Edit", model =>
        {
            model.Subject = string.IsNullOrWhiteSpace(settings.Subject) ? EmailAuthenticatorLoginSettings.DefaultSubject : settings.Subject;
            model.Body = string.IsNullOrWhiteSpace(settings.Body) ? EmailAuthenticatorLoginSettings.DefaultBody : settings.Body;
        }).Location("Content:10#Two-Factor Authentication")
        .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers), this)
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, EmailAuthenticatorLoginSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers))
        {
            return null;
        }

        var model = UserPolicySettingsEditor.Clone(settings);
        await context.Updater.TryUpdateModelAsync(model, Prefix);
        foreach (var error in UserPolicySettingsEditor.Validate(model, _liquidTemplateManager))
        {
            context.Updater.ModelState.AddModelError(Prefix, error.Key, string.Join(' ', error.Value));
        }
        if (context.Updater.ModelState.IsValid)
        {
            UserPolicySettingsEditor.Apply(settings, model);
        }

        return Edit(site, settings, context);
    }
}
