using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Drivers;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Services;
using OrchardCore.Themes.TheAdmin.ViewModels;

namespace OrchardCore.Themes.TheAdmin.Drivers;

/// <summary>
/// Shows and saves the admin theme's site settings on the Settings → Admin page, beside Orchard Core's own admin settings.
/// </summary>
public sealed class AdminThemeSettingsDisplayDriver : SiteDisplayDriver<AdminThemeSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly AdminSchemeService _schemes;

    internal readonly IStringLocalizer S;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminThemeSettingsDisplayDriver"/> class.
    /// </summary>
    public AdminThemeSettingsDisplayDriver(IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService, AdminSchemeService schemes, IStringLocalizer<AdminThemeSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _schemes = schemes;
        S = stringLocalizer;
    }

    /// <inheritdoc/>
    protected override string SettingsGroupId => AdminSiteSettingsDisplayDriver.GroupId;

    /// <inheritdoc/>
    public override IDisplayResult Edit(ISite site, AdminThemeSettings settings, BuildEditorContext context)
        => Initialize<AdminThemeSettingsViewModel>("AdminThemeSettings_Edit", async model =>
        {
            model.DefaultScheme = await _schemes.FindAsync(settings.DefaultScheme) ?? await _schemes.GetSiteSchemeAsync();
            model.Schemes = await _schemes.ListAsync();
        }).Location("Content:4")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AdminPermissions.ManageAdminSettings))
        .OnGroup(SettingsGroupId);

    /// <inheritdoc/>
    public override async Task<IDisplayResult> UpdateAsync(ISite site, AdminThemeSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AdminPermissions.ManageAdminSettings))
        {
            return null;
        }

        var model = new AdminThemeSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var scheme = await _schemes.FindAsync(model.DefaultScheme);

        if (scheme is null)
        {
            context.Updater.ModelState.AddModelError($"{Prefix}.{nameof(model.DefaultScheme)}", S["Pick one of the schemes."]);
        }
        else
        {
            settings.DefaultScheme = scheme;
        }

        return Edit(site, settings, context);
    }
}
