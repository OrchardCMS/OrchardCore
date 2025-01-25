using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Drivers;

public sealed class OpenIdValidationSettingsDisplayDriver : DisplayDriver<OpenIdValidationSettings>
{
    private readonly IShellHost _shellHost;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IStringLocalizer S;

    public OpenIdValidationSettingsDisplayDriver(
        IShellHost shellHost,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<OpenIdValidationSettingsDisplayDriver> stringLocalizer)
    {
        _shellHost = shellHost;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
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

        if (!string.IsNullOrEmpty(model.Tenant) && 
        (!_shellHost.TryGetShellContext(model.Tenant, out var shellContext) || !shellContext.Settings.IsRunning()))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Tenant), S["Invalid tenant value."]);
        } 
        else if (!hasAuthority)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Authority), S["A tenant or authority value is required."]);
        }

        return await EditAsync(settings, context);
    }
}
