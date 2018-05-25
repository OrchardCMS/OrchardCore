using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Security.Services;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers
{
    public class UserDisplayDriver : DisplayDriver<User>
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IUserService _userService;
        private readonly IRoleProvider _roleProvider;

        private readonly IStringLocalizer T;

        public UserDisplayDriver(
            UserManager<IUser> userManager,
            IUserService userService,
            IRoleProvider roleProvider,
            IStringLocalizer<UserDisplayDriver> stringLocalizer)
        {
            _userManager = userManager;
            _userService = userService;
            _roleProvider = roleProvider;

            T = stringLocalizer;
        }

        public override IDisplayResult Display(User user)
        {
            return Combine(
                Initialize<SummaryAdminUserViewModel>("UserFields", model => model.User = user).Location("SummaryAdmin", "Header:1"),
                Initialize<SummaryAdminUserViewModel>("UserButtons", model => model.User = user).Location("SummaryAdmin", "Actions:1")
            );
        }

        public override IDisplayResult Edit(User user)
        {
            return Initialize<EditUserViewModel>("UserFields_Edit", async model =>
            {
                var roleNames = await GetRoleNamesAsync();
                var userRoleNames = await _userManager.GetRolesAsync(user);
                var roles = roleNames.Select(x => new RoleViewModel { Role = x, IsSelected = userRoleNames.Contains(x, StringComparer.OrdinalIgnoreCase) }).ToArray();

                model.Id = await _userManager.GetUserIdAsync(user);
                model.UserName = await _userManager.GetUserNameAsync(user);
                model.Email = await _userManager.GetEmailAsync(user);
                model.Roles = roles;
                model.EmailConfirmed = user.EmailConfirmed;
                model.DisplayPasswordFields = await IsNewUser(model.Id);
            }).Location("Content:1");
        }

        public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            var model = new EditUserViewModel();

            if (!await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                return Edit(user);
            }

            model.UserName = model.UserName?.Trim();
            model.Email = model.Email?.Trim();

            if (await IsNewUser(model.Id))
            {
                if (model.Password != model.PasswordConfirmation)
                {
                    context.Updater.ModelState.AddModelError(nameof(model.PasswordConfirmation), T["Password and Password Confirmation do not match"]);
                }

                if (!context.Updater.ModelState.IsValid)
                {
                    return Edit(user);
                }

                var roleNames = model.Roles.Where(x => x.IsSelected).Select(x => x.Role).ToArray();
                await _userService.CreateUserAsync(model.UserName, model.Email, roleNames, model.Password, model.EmailConfirmed, (key, message) => context.Updater.ModelState.AddModelError(key, message));
            }
            else
            {
                var userWithSameName = await _userManager.FindByNameAsync(model.UserName);
                if (userWithSameName != null)
                {
                    var userWithSameNameId = await _userManager.GetUserIdAsync(userWithSameName);
                    if (userWithSameNameId != model.Id)
                    {
                        context.Updater.ModelState.AddModelError(string.Empty, T["The user name is already used."]);
                    }
                }

                var userWithSameEmail = await _userManager.FindByEmailAsync(model.Email);
                if (userWithSameEmail != null)
                {
                    var userWithSameEmailId = await _userManager.GetUserIdAsync(userWithSameEmail);
                    if (userWithSameEmailId != model.Id)
                    {
                        context.Updater.ModelState.AddModelError(string.Empty, T["The email is already used."]);
                    }
                }

                if (context.Updater.ModelState.IsValid)
                {
                    var roleNames = model.Roles.Where(x => x.IsSelected).Select(x => x.Role).ToList();
                    await _userManager.SetUserNameAsync(user, model.UserName);
                    await _userManager.SetEmailAsync(user, model.Email);

                    // Remove roles in two steps to prevent an iteration on a modified collection
                    var rolesToRemove = new List<string>();
                    foreach (var role in await _userManager.GetRolesAsync(user))
                    {
                        if (!roleNames.Contains(role))
                        {
                            rolesToRemove.Add(role);
                        }
                    }

                    foreach (var role in rolesToRemove)
                    {
                        await _userManager.RemoveFromRoleAsync(user, role);
                    }

                    // Add new roles
                    foreach (var role in roleNames)
                    {
                        if (!await _userManager.IsInRoleAsync(user, role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }

                    var result = await _userManager.UpdateAsync(user);
                    
                    foreach (var error in result.Errors)
                    {
                        context.Updater.ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return Edit(user);
        }

        private async Task<IEnumerable<string>> GetRoleNamesAsync()
        {
            var roleNames = await _roleProvider.GetRoleNamesAsync();
            return roleNames.Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<bool> IsNewUser(string userId)
        {
            return await _userManager.FindByIdAsync(userId) == null;
        }
    }
}
