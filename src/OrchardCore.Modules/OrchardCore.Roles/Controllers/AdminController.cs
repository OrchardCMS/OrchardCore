using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Data.Documents;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Infrastructure.Security;
using OrchardCore.Roles.Core;
using OrchardCore.Roles.ViewModels;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles.Controllers;

[Admin("Roles/{action}/{id?}", "Roles{action}")]
public sealed class AdminController : Controller
{
    private readonly IDocumentStore _documentStore;
    private readonly RoleManager<IRole> _roleManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IEnumerable<IPermissionProvider> _permissionProviders;
    private readonly ITypeFeatureProvider _typeFeatureProvider;
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly IRoleService _roleService;
    private readonly INotifier _notifier;
    private readonly IRoleTracker _roleTracker;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IDocumentStore documentStore,
        RoleManager<IRole> roleManager,
        IAuthorizationService authorizationService,
        IEnumerable<IPermissionProvider> permissionProviders,
        ITypeFeatureProvider typeFeatureProvider,
        IShellFeaturesManager shellFeaturesManager,
        IRoleService roleService,
        INotifier notifier,
        IRoleTracker roleTracker,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _documentStore = documentStore;
        _roleManager = roleManager;
        _authorizationService = authorizationService;
        _permissionProviders = permissionProviders;
        _typeFeatureProvider = typeFeatureProvider;
        _shellFeaturesManager = shellFeaturesManager;
        _roleService = roleService;
        _notifier = notifier;
        _roleTracker = roleTracker;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<ActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageRoles))
        {
            return Forbid();
        }

        var roles = await _roleService.GetRolesAsync();

        var model = new RolesViewModel
        {
            RoleEntries = roles.Select(role => new RoleEntry
            {
                Name = role.RoleName,
                Description = role.RoleDescription,
                Type = role.Type,
            }).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageRoles))
        {
            return Forbid();
        }

        var model = new CreateRoleViewModel();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageRoles))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            model.RoleName = model.RoleName.Trim();

            if (model.RoleName.Contains('/'))
            {
                ModelState.AddModelError(string.Empty, S["Invalid role name."]);
            }

            if (await _roleManager.FindByNameAsync(_roleManager.NormalizeKey(model.RoleName)) != null)
            {
                ModelState.AddModelError(string.Empty, S["The role is already used."]);
            }
        }

        if (ModelState.IsValid)
        {
            var role = new Role
            {
                RoleName = model.RoleName,
                RoleDescription = model.RoleDescription,
                Type = RoleHelper.SystemRoleNames.Contains(model.RoleName)
                ? RoleType.System
                : model.IsOwnerType ? RoleType.Owner : RoleType.Standard,
            };

            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                if (role.Type == RoleType.Owner)
                {
                    await _roleTracker.AddAsync(role);
                }
                await _notifier.SuccessAsync(H["Role created successfully."]);

                return RedirectToAction(nameof(Index));
            }

            await _documentStore.CancelAsync();

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form.
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageRoles))
        {
            return Forbid();
        }

        var currentRole = await _roleManager.FindByIdAsync(id);

        if (currentRole == null)
        {
            return NotFound();
        }

        if (currentRole.Type == RoleType.System)
        {
            await _notifier.ErrorAsync(H["System roles cannot be deleted."]);

            return RedirectToAction(nameof(Index));
        }

        var result = await _roleManager.DeleteAsync(currentRole);

        if (result.Succeeded)
        {
            if (currentRole.Type == RoleType.Owner)
            {
                await _roleTracker.RemoveAsync(currentRole);
            }

            await _notifier.SuccessAsync(H["Role deleted successfully."]);
        }
        else
        {
            await _documentStore.CancelAsync();

            await _notifier.ErrorAsync(H["Could not delete this role."]);

            foreach (var error in result.Errors)
            {
                await _notifier.ErrorAsync(H[error.Description]);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageRoles))
        {
            return Forbid();
        }

        if (await _roleManager.FindByNameAsync(_roleManager.NormalizeKey(id)) is not Role role)
        {
            return NotFound();
        }

        var installedPermissions = await GetInstalledPermissionsAsync();
        var allPermissions = installedPermissions.SelectMany(x => x.Value);

        var model = new EditRoleViewModel
        {
            Role = role,
            Name = role.RoleName,
            RoleDescription = role.RoleDescription,
            IsOwnerType = role.Type == RoleType.Owner,
            EffectivePermissions = await GetEffectivePermissions(role, allPermissions),
            RoleCategoryPermissions = installedPermissions
        };

        return View(model);
    }

    [HttpPost, ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPost(string id, string roleDescription, bool isOwnerType)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageRoles))
        {
            return Forbid();
        }

        if (await _roleManager.FindByNameAsync(_roleManager.NormalizeKey(id)) is not Role role)
        {
            return NotFound();
        }

        if (isOwnerType)
        {
            role.Type = RoleType.Owner;
        }

        role.RoleDescription = roleDescription;

        if (!isOwnerType)
        {
            var rolePermissions = new List<RoleClaim>();
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("Checkbox.", StringComparison.Ordinal) && Request.Form[key] == "true")
                {
                    var permissionName = key["Checkbox.".Length..];
                    rolePermissions.Add(new RoleClaim
                    {
                        ClaimType = Permission.ClaimType,
                        ClaimValue = permissionName,
                    });
                }
            }

            role.RoleClaims.RemoveAll(c => c.ClaimType == Permission.ClaimType);
            role.RoleClaims.AddRange(rolePermissions);
        }

        await _roleManager.UpdateAsync(role);

        // After updating the document manager, update the role tracker.
        if (role.Type == RoleType.Owner)
        {
            await _roleTracker.AddAsync(role);
        }
        else
        {
            await _roleTracker.RemoveAsync(role);
        }

        await _notifier.SuccessAsync(H["Role updated successfully."]);

        return RedirectToAction(nameof(Index));
    }

    private async Task<IDictionary<PermissionGroupKey, IEnumerable<Permission>>> GetInstalledPermissionsAsync()
    {
        var installedPermissions = new Dictionary<PermissionGroupKey, IEnumerable<Permission>>();
        var enabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();

        foreach (var permissionProvider in _permissionProviders)
        {
            // Two features could use the same permission.
            var feature = _typeFeatureProvider
                .GetFeaturesForDependency(permissionProvider.GetType())
                .LastOrDefault(feature => enabledFeatures.Any(enabledFeature => feature.Id == enabledFeature.Id));

            var permissions = await permissionProvider.GetPermissionsAsync();

            foreach (var permission in permissions)
            {
                var groupKey = GetGroupKey(feature, permission.Category);

                if (installedPermissions.TryGetValue(groupKey, out var value))
                {
                    installedPermissions[groupKey] = value.Concat(new[] { permission });

                    continue;
                }

                installedPermissions.Add(groupKey, new[] { permission });
            }
        }

        return installedPermissions;
    }

    private PermissionGroupKey GetGroupKey(IFeatureInfo feature, string category)
    {
        if (!string.IsNullOrWhiteSpace(category))
        {
            return new PermissionGroupKey(category, category);
        }

        var title = string.IsNullOrWhiteSpace(feature.Name) ? S["{0} Feature", feature.Id] : feature.Name;

        return new PermissionGroupKey(feature.Id, title)
        {
            Source = feature.Id,
        };
    }

    private async Task<IEnumerable<string>> GetEffectivePermissions(Role role, IEnumerable<Permission> allPermissions)
    {
        // Create a fake user to check the actual permissions. If the role is anonymous
        // IsAuthenticated needs to be false.
        var fakeIdentity = new ClaimsIdentity([new Claim(ClaimTypes.Role, role.RoleName)],
            !string.Equals(role.RoleName, OrchardCoreConstants.Roles.Anonymous, StringComparison.OrdinalIgnoreCase) ? "FakeAuthenticationType" : null);

        // Add role claims
        fakeIdentity.AddClaims(role.RoleClaims.Select(c => c.ToClaim()));

        var fakePrincipal = new ClaimsPrincipal(fakeIdentity);

        var result = new List<string>();

        foreach (var permission in allPermissions)
        {
            if (await _authorizationService.AuthorizeAsync(fakePrincipal, permission))
            {
                result.Add(permission.Name);
            }
        }

        return result;
    }
}
