using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers
{
    public class UserRoleDisplayDriver : DisplayDriver<User>
    {
        private const string AdministratorRole = "Administrator";

        private readonly UserManager<IUser> _userManager;
        private readonly IRoleService _roleService;
        private readonly IUserRoleStore<IUser> _userRoleStore;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        protected readonly IHtmlLocalizer H;

        public UserRoleDisplayDriver(
            UserManager<IUser> userManager,
            IRoleService roleService,
            IUserRoleStore<IUser> userRoleStore,
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            IAuthorizationService authorizationService,
            IHtmlLocalizer<UserRoleDisplayDriver> htmlLocalizer)
        {
            _userManager = userManager;
            _roleService = roleService;
            _userRoleStore = userRoleStore;
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            _authorizationService = authorizationService;
            H = htmlLocalizer;
        }

        public override IDisplayResult Display(User user)
        {
            return Combine(
                Initialize<SummaryAdminUserViewModel>("UserRolesMeta", model => model.User = user)
                    .Location("SummaryAdmin", "Description"),

                Initialize<SummaryAdminUserViewModel>("UserRoles", model => model.User = user)
                    .Location("DetailAdmin", "Content:10")
            );
        }

        public override IDisplayResult Edit(User user)
        {
            // This view is always rendered, however there will be no editable roles if the user does not have permission to edit them.
            return Initialize<EditUserRoleViewModel>("UserRoleFields_Edit", async model =>
            {
                // The current user can only view their roles if they have assign role, to prevent listing roles when managing their own profile.
                if (_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) == user.UserId
                    && !await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.AssignRoleToUsers))
                {
                    return;
                }

                var roles = await GetRoleAsync();

                // When a user is in a role that the current user cannot manage the role is shown but selection is disabled.
                var authorizedRoleNames = await GetAccessibleRoleNamesAsync(roles);
                var userRoleNames = await _userRoleStore.GetRolesAsync(user, default);

                var roleEntries = new List<RoleEntry>();
                foreach (var roleName in authorizedRoleNames)
                {
                    var roleEntry = new RoleEntry
                    {
                        Role = roleName,
                        IsSelected = userRoleNames.Contains(roleName, StringComparer.OrdinalIgnoreCase),
                    };

                    roleEntries.Add(roleEntry);
                }

                model.Roles = roleEntries.ToArray();
            })
            .Location("Content:1.10")
            .RenderWhen(async () => await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.EditUsers, user));
        }

        public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            var model = new EditUserRoleViewModel();

            // The current user cannot alter their own roles. This prevents them removing access to the site for themselves.
            if (_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) == user.UserId
                && !await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, StandardPermissions.SiteOwner))
            {
                await _notifier.WarningAsync(H["Cannot update your own roles."]);

                return Edit(user);
            }

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                var roles = await GetRoleAsync();
                // Authorize each role in the model to prevent html injection.
                var accessibleRoleNames = await GetAccessibleRoleNamesAsync(roles);
                var currentUserRoleNames = await _userRoleStore.GetRolesAsync(user, default);

                var selectedRoleNames = model.Roles.Where(x => x.IsSelected).Select(x => x.Role);
                var selectedRoles = roles.Where(x => selectedRoleNames.Contains(x.RoleName, StringComparer.OrdinalIgnoreCase));
                var accessibleAndSelectedRoleNames = await GetAccessibleRoleNamesAsync(selectedRoles);

                if (context.IsNew)
                {
                    // Only add authorized new roles.
                    foreach (var role in accessibleAndSelectedRoleNames)
                    {
                        await _userRoleStore.AddToRoleAsync(user, _userManager.NormalizeName(role), default);
                    }
                }
                else
                {
                    // Remove roles in two steps to prevent an iteration on a modified collection.
                    var rolesToRemove = new List<string>();
                    foreach (var role in currentUserRoleNames)
                    {
                        // When the user has permission to manage the role and it is no longer selected the role can be removed.
                        if (accessibleRoleNames.Contains(role, StringComparer.OrdinalIgnoreCase)
                            && !accessibleAndSelectedRoleNames.Contains(role, StringComparer.OrdinalIgnoreCase))
                        {
                            rolesToRemove.Add(role);
                        }
                    }

                    foreach (var role in rolesToRemove)
                    {
                        if (String.Equals(role, AdministratorRole, StringComparison.OrdinalIgnoreCase))
                        {
                            var enabledUsersOfAdminRole = (await _userManager.GetUsersInRoleAsync(AdministratorRole))
                                .Cast<User>()
                                .Where(user => user.IsEnabled)
                                .ToList();

                            // Make sure we always have at least one enabled administrator account.
                            if (enabledUsersOfAdminRole.Count == 1 && user.UserId == enabledUsersOfAdminRole.First().UserId)
                            {
                                await _notifier.WarningAsync(H[$"Cannot remove {AdministratorRole} role from the only enabled administrator."]);

                                continue;
                            }
                        }

                        await _userRoleStore.RemoveFromRoleAsync(user, _userManager.NormalizeName(role), default);
                    }

                    // Add new roles.
                    foreach (var role in accessibleAndSelectedRoleNames)
                    {
                        var normalizedName = _userManager.NormalizeName(role);
                        if (!await _userRoleStore.IsInRoleAsync(user, normalizedName, default))
                        {
                            await _userRoleStore.AddToRoleAsync(user, normalizedName, default);
                        }
                    }
                }
            }

            return Edit(user);
        }

        private async Task<IEnumerable<IRole>> GetRoleAsync()
        {
            var roles = await _roleService.GetRolesAsync();

            return roles.Where(role => !RoleHelper.SystemRoleNames.Contains(role.RoleName));
        }

        private async Task<IEnumerable<string>> GetAccessibleRoleNamesAsync(IEnumerable<IRole> roles)
        {
            var authorizedRoleNames = new List<string>();

            foreach (var role in roles)
            {
                if (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.AssignRoleToUsers, role))
                {
                    authorizedRoleNames.Add(role.RoleName);
                }
            }

            return authorizedRoleNames;
        }
    }
}
