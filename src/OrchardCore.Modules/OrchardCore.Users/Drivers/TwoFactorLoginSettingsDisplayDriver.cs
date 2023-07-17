using System;
using System.Threading.Tasks;
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

public class TwoFactorLoginSettingsDisplayDriver : SectionDisplayDriver<ISite, TwoFactorLoginSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    protected readonly IStringLocalizer S;

    public TwoFactorLoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<TwoFactorLoginSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(TwoFactorLoginSettings settings)
    {
        return Initialize<TwoFactorLoginSettings>("TwoFactorLoginSettings_Edit", model =>
        {
            model.NumberOfRecoveryCodesToGenerate = settings.NumberOfRecoveryCodesToGenerate;
            model.RequireTwoFactorAuthentication = settings.RequireTwoFactorAuthentication;
            model.AllowRememberClientTwoFactorAuthentication = settings.AllowRememberClientTwoFactorAuthentication;
        }).Location("Content:5#Two-Factor Authentication")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        .OnGroup(LoginSettingsDisplayDriver.GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(TwoFactorLoginSettings section, BuildEditorContext context)
    {
        if (!context.GroupId.Equals(LoginSettingsDisplayDriver.GroupId, StringComparison.OrdinalIgnoreCase)
            || !await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(section, Prefix);

        if (section.NumberOfRecoveryCodesToGenerate < 1)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(section.NumberOfRecoveryCodesToGenerate), S["Number of Recovery Codes to Generate should be grater than 0."]);
        }

        return Edit(section);
    }
}
