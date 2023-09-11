using System;
using System.Threading.Tasks;
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

public class EmailAuthenticatorLoginSettingsDisplayDriver : SectionDisplayDriver<ISite, EmailAuthenticatorLoginSettings>
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

    public override IDisplayResult Edit(EmailAuthenticatorLoginSettings settings)
    {
        return Initialize<EmailAuthenticatorLoginSettings>("EmailAuthenticatorLoginSettings_Edit", model =>
        {
            model.Subject = String.IsNullOrWhiteSpace(settings.Subject) ? EmailAuthenticatorLoginSettings.DefaultSubject : settings.Subject;
            model.Body = String.IsNullOrWhiteSpace(settings.Body) ? EmailAuthenticatorLoginSettings.DefaultBody : settings.Body;
        }).Location("Content:10#Two-Factor Authentication")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        .OnGroup(LoginSettingsDisplayDriver.GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(EmailAuthenticatorLoginSettings settings, BuildEditorContext context)
    {
        if (!context.GroupId.Equals(LoginSettingsDisplayDriver.GroupId, StringComparison.OrdinalIgnoreCase)
            || !await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        if (!_liquidTemplateManager.Validate(settings.Subject, out var subjectErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(settings.Subject), String.Join(' ', subjectErrors));
        }

        if (!_liquidTemplateManager.Validate(settings.Body, out var bodyErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(settings.Body), String.Join(' ', bodyErrors));
        }

        return Edit(settings);
    }
}
