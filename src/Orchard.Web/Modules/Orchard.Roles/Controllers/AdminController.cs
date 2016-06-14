using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Roles.Services;
using Orchard.Roles.ViewModels;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Settings;
using YesSql.Core.Services;

namespace Orchard.Roles.Controllers
{
    public class AdminController : Controller
    {
        private readonly ISession _session;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer T;
        private readonly ISiteService _siteService;
        private readonly IShapeFactory _shapeFactory;
        private readonly IRoleManager _roleManager;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        public AdminController(
            IAuthorizationService authorizationService,
            ITypeFeatureProvider typeFeatureProvider,
            ISession session,
            IStringLocalizer<AdminController> stringLocalizer,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IRoleManager roleManager,
            IEnumerable<IPermissionProvider> permissionProviders
            )
        {
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

            var roles = await _roleManager.GetRolesAsync();

            var model = new RolesViewModel
            {
                RoleEntries = roles.Roles.Select(BuildRoleEntry).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRoles))
            {
                return Unauthorized();
            }

            var role = await _roleManager.GetRoleByNameAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var model = new EditRoleViewModel
            {
                Role = role,
                Name = role.RoleName,
                EffectivePermissions = await GetEffectivePermissions(role),
                RoleCategoryPermissions = GetInstalledPermissions(),
            };


            return View(model);
        }

        private RoleEntry BuildRoleEntry(Role role)
        {
            return new RoleEntry
            {
                Name = role.RoleName,
                Selected = false
            };
        }

        private IDictionary<string, IEnumerable<Permission>> GetInstalledPermissions()
        {
            var installedPermissions = new Dictionary<string, IEnumerable<Permission>>();
            foreach (var permissionProvider in _permissionProviders)
            {
                var feature = _typeFeatureProvider.GetFeatureForDependency(permissionProvider.GetType());
                var featureName = feature.Descriptor.Id;
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

        private async Task<IEnumerable<string>> GetEffectivePermissions(Role role)
        {
            // Create a fake user to check the actual permissions
            var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role.RoleName) }));

            var permissionClaims = role.RoleClaims.Where(x => x.ClaimType == Permission.ClaimType);

            var result = new List<string>();

            foreach(var permission in permissionClaims)
            {
                if (await _authorizationService.AuthorizeAsync(fakeUser, new Permission(permission.ClaimValue)))
                {
                    result.Add(permission.ClaimValue);
                }
            }

            return result.Distinct();
        }
    }
}
