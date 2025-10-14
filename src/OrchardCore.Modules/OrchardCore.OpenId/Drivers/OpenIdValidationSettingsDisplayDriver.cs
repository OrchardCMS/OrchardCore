using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Drivers;

public sealed class OpenIdValidationSettingsDisplayDriver : DisplayDriver<OpenIdValidationSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public OpenIdValidationSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    public override async Task<IDisplayResult> EditAsync(OpenIdValidationSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, OpenIdPermissions.ManageValidationSettings))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<OpenIdValidationSettingsViewModel>("OpenIdValidationSettings_Edit", model =>
        {
            model.Authority = settings.Authority?.AbsoluteUri;
            model.MetadataAddress = settings.MetadataAddress?.AbsoluteUri;
            model.Audience = settings.Audience;
            model.DisableTokenTypeValidation = settings.DisableTokenTypeValidation;
            model.Tenant = settings.Tenant;
        }).Location("Content:2");
    }

    public override async Task<IDisplayResult> UpdateAsync(OpenIdValidationSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, OpenIdPermissions.ManageValidationSettings))
        {
            return null;
        }

        var model = new OpenIdValidationSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var hasAuthority = !string.IsNullOrEmpty(model.Authority);

        settings.Authority = hasAuthority ? new Uri(model.Authority, UriKind.Absolute) : null;
        settings.MetadataAddress = !string.IsNullOrEmpty(model.MetadataAddress) ? new Uri(model.MetadataAddress, UriKind.Absolute) : null;
        settings.Audience = model.Audience?.Trim();
        settings.DisableTokenTypeValidation = model.DisableTokenTypeValidation;
        settings.Tenant = model.Tenant;

        return await EditAsync(settings, context);
    }
}
