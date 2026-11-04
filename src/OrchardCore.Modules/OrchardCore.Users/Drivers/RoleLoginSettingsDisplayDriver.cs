using OrchardCore.Users.Services.Management;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Security.Services;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class RoleLoginSettingsDisplayDriver : SiteDisplayDriver<RoleLoginSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IRoleService _roleService;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => LoginSettingsDisplayDriver.GroupId;

    public RoleLoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IRoleService roleService,
        IStringLocalizer<RoleLoginSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _roleService = roleService;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, RoleLoginSettings settings, BuildEditorContext context)
    {
        return Initialize<RoleLoginSettingsViewModel>("LoginSettingsRoles_Edit", async model =>
        {
            model.RequireTwoFactorAuthenticationForSpecificRoles = settings.RequireTwoFactorAuthenticationForSpecificRoles;
            var roles = await _roleService.GetAssignableRolesAsync();

            model.Roles = roles
                .Select(role => new RoleEntry()
                {
                    Role = role.RoleName,
                    IsSelected = settings.Roles != null && settings.Roles.Contains(role.RoleName, StringComparer.OrdinalIgnoreCase),
                }).OrderBy(entry => entry.Role)
                .ToArray();
        }).Location("Content:6#Two-Factor Authentication")
        .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers), this)
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, RoleLoginSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers))
        {
            return null;
        }

        var model = new RoleLoginSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var proposed = UserPolicySettingsEditor.Clone(settings);
        proposed.RequireTwoFactorAuthenticationForSpecificRoles = model.RequireTwoFactorAuthenticationForSpecificRoles;
        if (model.RequireTwoFactorAuthenticationForSpecificRoles)
        {
            proposed.Roles = model.Roles?.Where(role => role.IsSelected).Select(role => role.Role).ToArray() ?? [];
        }
        foreach (var error in await UserPolicySettingsEditor.ValidateAsync(proposed, _roleService, S))
        {
            context.Updater.ModelState.AddModelError(Prefix, error.Key, string.Join(' ', error.Value));
        }
        if (context.Updater.ModelState.IsValid)
        {
            UserPolicySettingsEditor.Apply(settings, proposed);
        }

        return Edit(site, settings, context);
    }
}
