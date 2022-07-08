using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
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
        private readonly IHtmlLocalizer H;

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

        public override IDisplayResult Edit(User user)
        {
            // This view is always rendered, however there will be no editable roles if the user does not have permission to edit them.
            return Initialize<EditUserRoleViewModel>("UserRoleFields_Edit", async model =>
            {
                // The current user can only view their roles if they have list users, to prevent listing roles when managing their own profile.
                if (String.Equals(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), user.UserId, StringComparison.OrdinalIgnoreCase) &&
                    !await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ViewUsers))
                {
                    return;
                }

                var roleNames = await GetRoleNamesAsync();

                // When a user is in a role that the current user cannot manage the role is shown but selection is disabled.
                var authorizedRoleNames = await GetAuthorizedRoleNamesAsync(roleNames);
                var userRoleNames = await _userRoleStore.GetRolesAsync(user, default(CancellationToken));

                var roleEntries = new List<RoleEntry>();
                foreach (var roleName in roleNames)
                {
                    var roleEntry = new RoleEntry
                    {
                        Role = roleName,
                        IsSelected = userRoleNames.Contains(roleName, StringComparer.OrdinalIgnoreCase)
                    };

                    if (!authorizedRoleNames.Contains(roleName, StringComparer.OrdinalIgnoreCase))
                    {
                        roleEntry.IsEditingDisabled = true;
                    }

                    roleEntries.Add(roleEntry);
                }

                model.Roles = roleEntries.ToArray();
            })
            .Location("Content:1.10");
        }

        public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            var model = new EditUserRoleViewModel();

            // The current user cannot alter their own roles. This prevents them removing access to the site for themselves.
            if (String.Equals(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), user.UserId, StringComparison.OrdinalIgnoreCase))
            {
                return Edit(user);
            }

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                // Authorize each role in the model to prevent html injection.
                var authorizedRoleNames = await GetAuthorizedRoleNamesAsync(model.Roles.Select(x => x.Role));
                var userRoleNames = await _userRoleStore.GetRolesAsync(user, default(CancellationToken));

                var authorizedSelectedRoleNames = await GetAuthorizedRoleNamesAsync(model.Roles.Where(x => x.IsSelected).Select(x => x.Role));

                if (context.IsNew)
                {
                    // Only add authorized new roles.
                    foreach (var role in authorizedSelectedRoleNames)
                    {
                        await _userRoleStore.AddToRoleAsync(user, _userManager.NormalizeName(role), default(CancellationToken));
                    }
                }
                else
                {
                    // Remove roles in two steps to prevent an iteration on a modified collection
                    var rolesToRemove = new List<string>();
                    foreach (var role in userRoleNames)
                    {
                        // When the user has permission to manage the role and it is no longer selected the role can be removed.
                        if (authorizedRoleNames.Contains(role, StringComparer.OrdinalIgnoreCase) && !authorizedSelectedRoleNames.Contains(role, StringComparer.OrdinalIgnoreCase))
                        {
                            rolesToRemove.Add(role);
                        }
                    }
                    foreach (var role in rolesToRemove)
                    {
                        if (String.Equals(role, AdministratorRole, StringComparison.OrdinalIgnoreCase))
                        {
                            var usersOfAdminRole = (await _userManager.GetUsersInRoleAsync(AdministratorRole)).Cast<User>();
                            // Make sure we always have at least one administrator account
                            if (usersOfAdminRole.Count() == 1 && String.Equals(user.UserId, usersOfAdminRole.First().UserId, StringComparison.OrdinalIgnoreCase))
                            {
                                await _notifier.WarningAsync(H["Cannot remove administrator role from the only administrator."]);
                                continue;
                            }
                            else
                            {
                                await _userRoleStore.RemoveFromRoleAsync(user, _userManager.NormalizeName(role), default(CancellationToken));
                            }
                        }
                        else
                        {
                            await _userRoleStore.RemoveFromRoleAsync(user, _userManager.NormalizeName(role), default(CancellationToken));
                        }
                    }

                    // Add new roles
                    foreach (var role in authorizedSelectedRoleNames)
                    {
                        if (!await _userRoleStore.IsInRoleAsync(user, _userManager.NormalizeName(role), default(CancellationToken)))
                        {
                            await _userRoleStore.AddToRoleAsync(user, _userManager.NormalizeName(role), default(CancellationToken));
                        }
                    }
                }
            }

            return Edit(user);
        }

        private async Task<IEnumerable<string>> GetRoleNamesAsync()
        {
            var roleNames = await _roleService.GetRoleNamesAsync();
            return roleNames.Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<IEnumerable<string>> GetAuthorizedRoleNamesAsync(IEnumerable<string> roleNames)
        {
            var authorizedRoleNames = new List<string>();
            foreach (var roleName in roleNames)
            {
                if (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, OrchardCore.Roles.CommonPermissions.CreatePermissionForAssignRole(roleName)))
                {
                    authorizedRoleNames.Add(roleName);
                }
            }

            return authorizedRoleNames;
        }
    }
}
