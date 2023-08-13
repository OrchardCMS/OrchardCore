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

public class SmsAuthenticatorLoginSettingsDisplayDriver : SectionDisplayDriver<ISite, SmsAuthenticatorLoginSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;

    public SmsAuthenticatorLoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        ILiquidTemplateManager liquidTemplateManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _liquidTemplateManager = liquidTemplateManager;
    }

    public override IDisplayResult Edit(SmsAuthenticatorLoginSettings settings)
    {
        return Initialize<SmsAuthenticatorLoginSettings>("SmsAuthenticatorLoginSettings_Edit", model =>
        {
            model.Body = String.IsNullOrWhiteSpace(settings.Body)
            ? EmailAuthenticatorLoginSettings.DefaultBody
            : settings.Body;
        }).Location("Content:15#Two-Factor Authentication")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        .OnGroup(LoginSettingsDisplayDriver.GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(SmsAuthenticatorLoginSettings settings, BuildEditorContext context)
    {
        if (!context.GroupId.Equals(LoginSettingsDisplayDriver.GroupId, StringComparison.OrdinalIgnoreCase)
            || !await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        if (!_liquidTemplateManager.Validate(settings.Body, out var bodyErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(settings.Body), String.Join(' ', bodyErrors));
        }

        return Edit(settings);
    }
}
