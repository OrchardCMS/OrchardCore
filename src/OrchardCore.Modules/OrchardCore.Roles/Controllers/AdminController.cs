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
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<ActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, RolesPermissions.ManageRoles))
        {
            return Forbid();
        }

        var roles = await _roleService.GetRolesAsync();

        var model = new RolesViewModel()
        {
            RoleEntries = [],
        };

        foreach (var role in roles.OrderBy(r => r.RoleName))
        {
            model.RoleEntries.Add(new RoleEntry
            {
                Name = role.RoleName,
                Description = role.RoleDescription,
                IsSystemRole = await _roleService.IsSystemRoleAsync(role.RoleName),
            });
        }

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, RolesPermissions.ManageRoles))
        {
            return Forbid();
        }

        var model = new CreateRoleViewModel();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RolesPermissions.ManageRoles))
        {
            return Forbid();
        }

        if (ModelState.IsValid && await ValidateRoleNameAsync(model.RoleName))
        {
            var role = new Role
            {
                RoleName = model.RoleName,
                RoleDescription = model.RoleDescription,
            };

            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
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

    public async Task<IActionResult> Edit(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RolesPermissions.ManageRoles))
        {
            return Forbid();
        }

        if (await _roleManager.FindByIdAsync(id) is not Role role)
        {
            return NotFound();
        }

        var model = new EditRoleViewModel
        {
            Role = role,
            Name = role.RoleName,
            RoleDescription = role.RoleDescription,
            IsAdminRole = await _roleService.IsAdminRoleAsync(role.RoleName),
        };

        var installedPermissions = await GetInstalledPermissionsAsync();
        var allPermissions = installedPermissions.SelectMany(x => x.Value);

        ViewData["DuplicatedPermissions"] = allPermissions
            .GroupBy(p => p.Name.ToUpperInvariant())
            .Where(g => g.Count() > 1)
            .Select(g => g.First().Name)
            .ToArray();

        if (!await _roleService.IsAdminRoleAsync(role.RoleName))
        {
            model.EffectivePermissions = await GetEffectivePermissions(role, allPermissions);
            model.RoleCategoryPermissions = installedPermissions;
        }

        return View(model);
    }

    [HttpPost, ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPost(string id, string roleDescription)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RolesPermissions.ManageRoles))
        {
            return Forbid();
        }

        if (await _roleManager.FindByIdAsync(id) is not Role role)
        {
            return NotFound();
        }

        role.RoleDescription = roleDescription;

        if (!await _roleService.IsAdminRoleAsync(role.RoleName))
        {
            role.RoleClaims.RemoveAll(c => c.ClaimType == Permission.ClaimType);
            role.RoleClaims.AddRange(ExtractSelectedPermissions());
        }

        await _roleManager.UpdateAsync(role);

        await _notifier.SuccessAsync(H["Role updated successfully."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RolesPermissions.ManageRoles))
        {
            return Forbid();
        }

        var role = await _roleManager.FindByIdAsync(id);

        if (role == null)
        {
            return NotFound();
        }

        if (await _roleService.IsSystemRoleAsync(role.RoleName))
        {
            await _notifier.ErrorAsync(H["System roles cannot be deleted."]);

            return RedirectToAction(nameof(Index));
        }

        var result = await _roleManager.DeleteAsync(role);

        if (result.Succeeded)
        {
            await _notifier.SuccessAsync(H["Role deleted successfully."]);
        }
        else
        {
            await _documentStore.CancelAsync();

            await _notifier.ErrorAsync(H["Could not delete this role."]);

            foreach (var error in result.Errors)
            {
                await _notifier.ErrorAsync(new LocalizedHtmlString(error.Description, error.Description));
            }
        }

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

    public async Task<IActionResult> Clone(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RolesPermissions.ManageRoles))
        {
            return Forbid();
        }

        if (await _roleManager.FindByIdAsync(id) is not Role role)
        {
            return NotFound();
        }

        var model = await BuildRoleViewModelAsync(role, role.RoleName, role.RoleDescription);
        return View("Edit", model);
    }

    [HttpPost, ActionName(nameof(Clone))]
    public async Task<IActionResult> ClonePost(string id, string cloneRoleName, string roleDescription)
    {
        if (!await _authorizationService.AuthorizeAsync(User, RolesPermissions.ManageRoles))
        {
            return Forbid();
        }

        if (await _roleManager.FindByIdAsync(id) is not Role sourceRole)
        {
            return NotFound();
        }

        cloneRoleName = cloneRoleName?.Trim();

        if (await ValidateRoleNameAsync(cloneRoleName))
        {
            var newRole = new Role
            {
                RoleName = cloneRoleName,
                RoleDescription = roleDescription,
                RoleClaims = await _roleService.IsAdminRoleAsync(cloneRoleName)
                    ? new List<RoleClaim>()
                    : ExtractSelectedPermissions(),
            };

            var result = await _roleManager.CreateAsync(newRole);

            if (result.Succeeded)
            {
                await _notifier.SuccessAsync(H["Role cloned successfully."]);
                return RedirectToAction(nameof(Index));
            }

            await _documentStore.CancelAsync();

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        var model = await BuildRoleViewModelAsync(sourceRole, cloneRoleName, roleDescription);
        return View("Edit", model);
    }

    // Common role name validation
    private async Task<bool> ValidateRoleNameAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            ModelState.AddModelError(string.Empty, S["Role name is required."]);
            return false;
        }

        roleName = roleName.Trim();

        if (roleName.Contains('/'))
        {
            ModelState.AddModelError(string.Empty, S["Invalid role name."]);
            return false;
        }

        if (await _roleManager.FindByNameAsync(roleName) != null)
        {
            ModelState.AddModelError(string.Empty, S["The role name is already in use."]);
            return false;
        }

        return true;
    }

    // Common permission extraction
    private List<RoleClaim> ExtractSelectedPermissions()
    {
        return Request.Form.Keys
            .Where(key => key.StartsWith("Checkbox.", StringComparison.Ordinal) && Request.Form[key] == "true")
            .Select(key => RoleClaim.Create(key["Checkbox.".Length..]))
            .ToList();
    }

    // Common Edit/Clone ViewModel builder
    private async Task<EditRoleViewModel> BuildRoleViewModelAsync(Role role, string cloneRoleName, string description)
    {
        var installedPermissions = await GetInstalledPermissionsAsync();
        var allPermissions = installedPermissions.SelectMany(x => x.Value);
        var isAdmin = await _roleService.IsAdminRoleAsync(role.RoleName);

        var model = new EditRoleViewModel
        {
            Role = role,
            Name = role.RoleName,
            CloneRoleName = cloneRoleName,
            RoleDescription = description,
            IsAdminRole = isAdmin,
            IsCloneRole = true,
            RoleCategoryPermissions = installedPermissions,
            EffectivePermissions = !isAdmin ? await GetEffectivePermissions(role, allPermissions) : null,
        };

        ViewData["DuplicatedPermissions"] = allPermissions
            .GroupBy(p => p.Name.ToUpperInvariant())
            .Where(g => g.Count() > 1)
            .Select(g => g.First().Name)
            .ToArray();

        return model;
    }
}
