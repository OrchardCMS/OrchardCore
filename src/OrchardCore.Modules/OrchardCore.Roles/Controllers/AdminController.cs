using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Data.Documents;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Roles.ViewModels;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles.Controllers
{
    public class AdminController : Controller
    {
        private readonly IDocumentStore _documentStore;
        private readonly RoleManager<IRole> _roleManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly IRoleService _roleService;
        private readonly INotifier _notifier;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;

        public AdminController(
            IDocumentStore documentStore,
            RoleManager<IRole> roleManager,
            IAuthorizationService authorizationService,
            IEnumerable<IPermissionProvider> permissionProviders,
            ITypeFeatureProvider typeFeatureProvider,
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
            _roleService = roleService;
            _notifier = notifier;
            S = stringLocalizer;
            H = htmlLocalizer;
        }

        public async Task<ActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRoles))
            {
                return Forbid();
            }

            var roles = await _roleService.GetRolesAsync();

            var model = new RolesViewModel
            {
                RoleEntries = roles.Select(BuildRoleEntry).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRoles))
            {
                return Forbid();
            }

            var model = new CreateRoleViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRoles))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                model.RoleName = model.RoleName.Trim();

                if (model.RoleName.Contains('/'))
                {
                    ModelState.AddModelError(String.Empty, S["Invalid role name."]);
                }

                if (await _roleManager.FindByNameAsync(_roleManager.NormalizeKey(model.RoleName)) != null)
                {
                    ModelState.AddModelError(String.Empty, S["The role is already used."]);
                }
            }

            if (ModelState.IsValid)
            {
                var role = new Role { RoleName = model.RoleName, RoleDescription = model.RoleDescription };
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    await _notifier.SuccessAsync(H["Role created successfully."]);
                    return RedirectToAction(nameof(Index));
                }

                await _documentStore.CancelAsync();

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(String.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRoles))
            {
                return Forbid();
            }

            var currentRole = await _roleManager.FindByIdAsync(id);

            if (currentRole == null)
            {
                return NotFound();
            }

            var result = await _roleManager.DeleteAsync(currentRole);

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
                    await _notifier.ErrorAsync(H[error.Description]);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRoles))
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
                EffectivePermissions = await GetEffectivePermissions(role, allPermissions),
                RoleCategoryPermissions = installedPermissions
            };

            return View(model);
        }

        [HttpPost, ActionName(nameof(Edit))]
        public async Task<IActionResult> EditPost(string id, string roleDescription)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRoles))
            {
                return Forbid();
            }

            if (await _roleManager.FindByNameAsync(_roleManager.NormalizeKey(id)) is not Role role)
            {
                return NotFound();
            }

            role.RoleDescription = roleDescription;

            // Save.
            var rolePermissions = new List<RoleClaim>();
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("Checkbox.", StringComparison.Ordinal) && Request.Form[key] == "true")
                {
                    var permissionName = key["Checkbox.".Length..];
                    rolePermissions.Add(new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = permissionName });
                }
            }

            role.RoleClaims.RemoveAll(c => c.ClaimType == Permission.ClaimType);
            role.RoleClaims.AddRange(rolePermissions);

            await _roleManager.UpdateAsync(role);

            await _notifier.SuccessAsync(H["Role updated successfully."]);

            return RedirectToAction(nameof(Index));
        }

        private RoleEntry BuildRoleEntry(IRole role)
        {
            return new RoleEntry
            {
                Name = role.RoleName,
                Description = role.RoleDescription,
                Selected = false
            };
        }

        private async Task<IDictionary<PermissionGroupKey, IEnumerable<Permission>>> GetInstalledPermissionsAsync()
        {
            var installedPermissions = new Dictionary<PermissionGroupKey, IEnumerable<Permission>>();
            foreach (var permissionProvider in _permissionProviders)
            {
                var feature = _typeFeatureProvider.GetFeatureForDependency(permissionProvider.GetType());
                var permissions = await permissionProvider.GetPermissionsAsync();

                foreach (var permission in permissions)
                {
                    var groupKey = GetGroupKey(feature, permission.Category);

                    if (installedPermissions.ContainsKey(groupKey))
                    {
                        installedPermissions[groupKey] = installedPermissions[groupKey].Concat(new[] { permission });

                        continue;
                    }

                    installedPermissions.Add(groupKey, new[] { permission });
                }
            }

            return installedPermissions;
        }

        private PermissionGroupKey GetGroupKey(IFeatureInfo feature, string category)
        {
            if (!String.IsNullOrWhiteSpace(category))
            {
                return new PermissionGroupKey(category, category);
            }

            var title = String.IsNullOrWhiteSpace(feature.Name) ? S["{0} Feature", feature.Id] : feature.Name;

            return new PermissionGroupKey(feature.Id, title)
            {
                Source = feature.Id,
            };
        }

        private async Task<IEnumerable<string>> GetEffectivePermissions(Role role, IEnumerable<Permission> allPermissions)
        {
            // Create a fake user to check the actual permissions. If the role is anonymous
            // IsAuthenticated needs to be false.
            var fakeIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role.RoleName) },
                role.RoleName != "Anonymous" ? "FakeAuthenticationType" : null);

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
}
