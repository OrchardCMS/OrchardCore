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
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions;
using OrchardCore.Roles.ViewModels;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Roles.Controllers
{
    public class AdminController : Controller
    {
        private readonly ISession _session;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer T;
        private readonly ISiteService _siteService;
        private readonly IShapeFactory _shapeFactory;
        private readonly RoleManager<IRole> _roleManager;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly IRoleProvider _roleProvider;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<AdminController> TH;

        public AdminController(
            IAuthorizationService authorizationService,
            ITypeFeatureProvider typeFeatureProvider,
            ISession session,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            RoleManager<IRole> roleManager,
            IRoleProvider roleProvider,
            INotifier notifier,
            IEnumerable<IPermissionProvider> permissionProviders
            )
        {
            TH = htmlLocalizer;
            _notifier = notifier;
            _roleProvider = roleProvider;
            _typeFeatureProvider = typeFeatureProvider;
            _permissionProviders = permissionProviders;
            _roleManager = roleManager;
            _shapeFactory = shapeFactory;
            _siteService = siteService;
            T = stringLocalizer;
            _authorizationService = authorizationService;
            _session = session;
        }

        public async Task<ActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRoles))
            {
                return Unauthorized();
            }

            var roles = await _roleProvider.GetRoleNamesAsync();

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
                return Unauthorized();
            }

            var model = new CreateRoleViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.RoleName = model.RoleName.Trim();
                
                if (await _roleManager.FindByNameAsync(_roleManager.NormalizeKey(model.RoleName)) != null)
                {
                    ModelState.AddModelError(string.Empty, T["The role is already used."]);
                }
            }

            if (ModelState.IsValid)
            {
                var role = new Role { RoleName = model.RoleName };
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    _notifier.Success(TH["Role created successfully"]);
                    return RedirectToAction(nameof(Index));
                }

                _session.Cancel();

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
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
                return Unauthorized();
            }

            var currentRole = await _roleManager.FindByIdAsync(id);

            if (currentRole == null)
            {
                return NotFound();
            }

            var result = await _roleManager.DeleteAsync(currentRole);

            if (result.Succeeded)
            {
                _notifier.Success(TH["Role deleted successfully"]);
            }
            else
            {
                _session.Cancel();

                _notifier.Error(TH["Could not delete this role"]);

                foreach (var error in result.Errors)
                {
                    _notifier.Error(TH[error.Description]);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRoles))
            {
                return Unauthorized();
            }

            var role = (Role) await _roleManager.FindByNameAsync(_roleManager.NormalizeKey(id));
            if (role == null)
            {
                return NotFound();
            }

            var installedPermissions = GetInstalledPermissions();
            var allPermissions = installedPermissions.SelectMany(x => x.Value);

            var model = new EditRoleViewModel
            {
                Role = role,
                Name = role.RoleName,
                EffectivePermissions = await GetEffectivePermissions(role, allPermissions),
                RoleCategoryPermissions = installedPermissions
            };

            return View(model);
        }

        [HttpPost, ActionName(nameof(Edit))]
        public async Task<IActionResult> EditPost(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRoles))
            {
                return Unauthorized();
            }

            var role = (Role) await _roleManager.FindByNameAsync(_roleManager.NormalizeKey(id));

            if (role == null)
            {
                return NotFound();
            }

            // Save
            List<RoleClaim> rolePermissions = new List<RoleClaim>();
            foreach (string key in Request.Form.Keys)
            {
                if (key.StartsWith("Checkbox.") && Request.Form[key] == "true")
                {
                    string permissionName = key.Substring("Checkbox.".Length);
                    rolePermissions.Add(new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = permissionName });
                }
            }

            role.RoleClaims.RemoveAll(c => c.ClaimType == Permission.ClaimType);
            role.RoleClaims.AddRange(rolePermissions);

            await _roleManager.UpdateAsync(role);

            _notifier.Success(TH["Role updated successfully."]);

            return RedirectToAction(nameof(Index));
        }

        private RoleEntry BuildRoleEntry(string name)
        {
            return new RoleEntry
            {
                Name = name,
                Selected = false
            };
        }

        private IDictionary<string, IEnumerable<Permission>> GetInstalledPermissions()
        {
            var installedPermissions = new Dictionary<string, IEnumerable<Permission>>();
            foreach (var permissionProvider in _permissionProviders)
            {
                var feature = _typeFeatureProvider.GetFeatureForDependency(permissionProvider.GetType());
                var featureName = feature.Id;
                var permissions = permissionProvider.GetPermissions();
                foreach (var permission in permissions)
                {
                    var category = permission.Category;

                    string title = String.IsNullOrWhiteSpace(category) ? T["{0} Feature", featureName] : category;

                    if (installedPermissions.ContainsKey(title))
                    {
                        installedPermissions[title] = installedPermissions[title].Concat(new[] { permission });
                    }
                    else
                    {
                        installedPermissions.Add(title, new[] { permission });
                    }
                }
            }

            return installedPermissions;
        }

        private async Task<IEnumerable<string>> GetEffectivePermissions(Role role, IEnumerable<Permission> allPermissions)
        {
            // Create a fake user to check the actual permissions. If the role is anonymous
            // IsAuthenticated needs to be false.
            var fakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role.RoleName)},
                role.RoleName != "Anonymous" ? "FakeAuthenticationType" : null)
            );

            var result = new List<string>();

            foreach(var permission in allPermissions)
            {
                if (await _authorizationService.AuthorizeAsync(fakeUser, permission))
                {
                    result.Add(permission.Name);
                }
            }

            return result;
        }
    }
}
