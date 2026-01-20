using System;
using System.Linq;
using System.Threading.Tasks;
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

public class RoleLoginSettingsDisplayDriver : SectionDisplayDriver<ISite, RoleLoginSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IRoleService _roleService;
    protected readonly IStringLocalizer S;

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

    public override IDisplayResult Edit(RoleLoginSettings settings)
    {
        return Initialize<RoleLoginSettingsViewModel>("LoginSettingsRoles_Edit", async model =>
        {
            model.RequireTwoFactorAuthenticationForSpecificRoles = settings.RequireTwoFactorAuthenticationForSpecificRoles;
            var roles = await _roleService.GetRolesAsync();
            model.Roles = roles.Select(role => new RoleEntry()
            {
                Role = role.RoleName,
                IsSelected = settings.Roles != null && settings.Roles.Contains(role.RoleName),
            }).OrderBy(entry => entry.Role)
            .ToArray();
        }).Location("Content:6#Two-Factor Authentication")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        .OnGroup(LoginSettingsDisplayDriver.GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(RoleLoginSettings settings, BuildEditorContext context)
    {
        if (!context.GroupId.Equals(LoginSettingsDisplayDriver.GroupId, StringComparison.OrdinalIgnoreCase)
            || !await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        {
            return null;
        }

        var model = new RoleLoginSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.RequireTwoFactorAuthenticationForSpecificRoles)
        {
            var roles = await _roleService.GetRolesAsync();

            var selectedRoles = model.Roles.Where(x => x.IsSelected)
                .Join(roles, e => e.Role, r => r.RoleName, (e, r) => r.RoleName)
                .ToArray();

            if (selectedRoles.Length == 0)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Roles), S["Select at least one role."]);
            }
            else
            {
                settings.RequireTwoFactorAuthenticationForSpecificRoles = true;
                settings.Roles = selectedRoles;
            }
        }
        else
        {
            settings.RequireTwoFactorAuthenticationForSpecificRoles = false;
        }

        return Edit(settings);
    }
}
