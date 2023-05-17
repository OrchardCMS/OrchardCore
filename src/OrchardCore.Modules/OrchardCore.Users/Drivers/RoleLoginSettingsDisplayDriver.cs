using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

public class RoleLoginSettingsDisplayDriver : SectionDisplayDriver<ISite, LoginSettings>
{
    public const string GroupId = "userLogin";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IRoleService _roleService;
    private readonly IStringLocalizer S;

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

    public override async Task<IDisplayResult> EditAsync(LoginSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, CommonPermissions.ManageUsers))
        {
            return null;
        }

        return Initialize<RoleLoginSettingsViewModel>("LoginSettingsRoles_Edit", async model =>
        {
            model.EnableTwoFactorAuthentication = settings.EnableTwoFactorAuthentication;
            model.EnableTwoFactorAuthenticationForSpecificRoles = settings.EnableTwoFactorAuthenticationForSpecificRoles;
            var roles = await _roleService.GetRolesAsync();
            model.Roles = roles.Select(role => new RoleEntry()
            {
                Role = role.RoleName,
                IsSelected = settings.Roles != null && settings.Roles.Contains(role.RoleName),
            }).OrderBy(entry => entry.Role)
            .ToArray();
        }).Location("Content:5.1#Two-factor Authentication")
        .OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(LoginSettings section, BuildEditorContext context)
    {
        if (context.GroupId != GroupId || !await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        {
            return null;
        }

        var model = new RoleLoginSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.EnableTwoFactorAuthenticationForSpecificRoles
            && (model.Roles == null || !model.Roles.Any(x => x.IsSelected)))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Roles), S["Select at least one role."]);
        }

        if (model.EnableTwoFactorAuthenticationForSpecificRoles)
        {
            section.EnableTwoFactorAuthenticationForSpecificRoles = true;
            section.Roles = model.Roles.Where(x => x.IsSelected)
                .Select(x => x.Role)
                .ToArray();
        }
        else
        {
            section.EnableTwoFactorAuthenticationForSpecificRoles = false;
        }

        return await EditAsync(section, context);
    }
}
