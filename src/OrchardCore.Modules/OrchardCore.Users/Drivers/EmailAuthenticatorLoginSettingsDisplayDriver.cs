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
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, EmailAuthenticatorLoginSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        if (!_liquidTemplateManager.Validate(settings.Subject, out var subjectErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(settings.Subject), string.Join(' ', subjectErrors));
        }

        if (!_liquidTemplateManager.Validate(settings.Body, out var bodyErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(settings.Body), string.Join(' ', bodyErrors));
        }

        return Edit(site, settings, context);
    }
}
