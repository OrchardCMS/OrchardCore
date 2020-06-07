using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private readonly IUserStore<IUser> _userStore;
        private readonly IUserEmailStore<IUser> _userEmailStore;
        private readonly IUserRoleStore<IUser> _userRoleStore;
        private readonly IStringLocalizer T;

        public UserDisplayDriver(
            UserManager<IUser> userManager,
            IUserService userService,
            IRoleProvider roleProvider,
            IUserStore<IUser> userStore,
            IUserEmailStore<IUser> userEmailStore,
            IUserRoleStore<IUser> userRoleStore,
            IStringLocalizer<UserDisplayDriver> stringLocalizer)
        {
            _userManager = userManager;
            _userService = userService;
            _roleProvider = roleProvider;
            _userStore = userStore;
            _userEmailStore = userEmailStore;
            _userRoleStore = userRoleStore;
            T = stringLocalizer;
        }

        public override IDisplayResult Display(User user)
        {
            return Combine(
                Initialize<SummaryAdminUserViewModel>("UserFields", model => model.User = user).Location("SummaryAdmin", "Header:1"),
                Initialize<SummaryAdminUserViewModel>("UserButtons", model => model.User = user).Location("SummaryAdmin", "Actions:1")
            );
        }

        public override Task<IDisplayResult> EditAsync(User user, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(Initialize<EditUserViewModel>("UserFields_Edit", async model =>
            {
                var roleNames = await GetRoleNamesAsync();
                var userRoleNames = await _userManager.GetRolesAsync(user);
                var roles = roleNames.Select(x => new RoleViewModel { Role = x, IsSelected = userRoleNames.Contains(x, StringComparer.OrdinalIgnoreCase) }).ToArray();

                model.Id = await _userManager.GetUserIdAsync(user);
                model.UserName = await _userManager.GetUserNameAsync(user);
                model.Email = await _userManager.GetEmailAsync(user);
                model.Roles = roles;
                model.EmailConfirmed = user.EmailConfirmed;
                model.IsNewUser = user.Id == 0 ? true : false; 
            }).Location("Content:1"));
        }

        public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            var model = new EditUserViewModel();

            if (!await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                return await EditAsync(user, context);
            }

            model.UserName = model.UserName?.Trim();
            model.Email = model.Email?.Trim();
            user.EmailConfirmed = model.EmailConfirmed;

            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                context.Updater.ModelState.AddModelError("UserName", T["A user name is required."]);
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                context.Updater.ModelState.AddModelError("Email", T["An email is required."]);
            }

            await _userStore.SetUserNameAsync(user, model.UserName, default(CancellationToken));
            await _userEmailStore.SetEmailAsync(user, model.Email, default(CancellationToken));

            var userWithSameName = await _userStore.FindByNameAsync(_userManager.NormalizeKey(model.UserName), default(CancellationToken));
            if (userWithSameName != null)
            {
                var userWithSameNameId = await _userStore.GetUserIdAsync(userWithSameName, default(CancellationToken));
                if (userWithSameNameId != model.Id)
                {
                    context.Updater.ModelState.AddModelError(string.Empty, T["The user name is already used."]);
                }
            }

            var userWithSameEmail = await _userEmailStore.FindByEmailAsync(_userManager.NormalizeKey(model.Email), default(CancellationToken));
            if (userWithSameEmail != null)
            {
                var userWithSameEmailId = await _userStore.GetUserIdAsync(userWithSameEmail, default(CancellationToken));
                if (userWithSameEmailId != model.Id)
                {
                    context.Updater.ModelState.AddModelError(string.Empty, T["The email is already used."]);
                }
            }

            if (context.Updater.ModelState.IsValid)
            {
                if (model.GeneratePassword)
                {
                    var password = GenerateRandomPassword();
                    await _userManager.AddPasswordAsync(user, password);
                }

                var roleNames = model.Roles.Where(x => x.IsSelected).Select(x => x.Role).ToList();

                if (context.IsNew)
                {
                    // Add new roles
                    foreach (var role in roleNames)
                    {
                        await _userRoleStore.AddToRoleAsync(user, _userManager.NormalizeKey(role), default(CancellationToken));
                    }
                }
                else
                {
                    // Remove roles in two steps to prevent an iteration on a modified collection
                    var rolesToRemove = new List<string>();
                    foreach (var role in await _userRoleStore.GetRolesAsync(user, default(CancellationToken)))
                    {
                        if (!roleNames.Contains(role))
                        {
                            rolesToRemove.Add(role);
                        }
                    }

                    foreach (var role in rolesToRemove)
                    {
                        await _userRoleStore.RemoveFromRoleAsync(user, _userManager.NormalizeKey(role), default(CancellationToken));
                    }

                    // Add new roles
                    foreach (var role in roleNames)
                    {
                        if (!await _userRoleStore.IsInRoleAsync(user, _userManager.NormalizeKey(role), default(CancellationToken)))
                        {
                            await _userRoleStore.AddToRoleAsync(user, _userManager.NormalizeKey(role), default(CancellationToken));
                        }
                    }
                }
            }

            return await EditAsync(user, context);
        }

        private async Task<IEnumerable<string>> GetRoleNamesAsync()
        {
            var roleNames = await _roleProvider.GetRoleNamesAsync();
            return roleNames.Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase);
        }

                private string GenerateRandomPassword()
        {
            var options = _userManager.Options.Password;

            var length = options.RequiredLength;
            var nonAlphanumeric = options.RequireNonAlphanumeric;
            var digit = options.RequireDigit;
            var lowercase = options.RequireLowercase;
            var uppercase = options.RequireUppercase;

            var password = new StringBuilder();
            Random random = new Random();

            while (password.Length < length)
            {
                char c = (char)random.Next(32, 126);

                password.Append(c);

                if (char.IsDigit(c))
                    digit = false;
                else if (char.IsLower(c))
                    lowercase = false;
                else if (char.IsUpper(c))
                    uppercase = false;
                else if (!char.IsLetterOrDigit(c))
                    nonAlphanumeric = false;
            }

            if (nonAlphanumeric)
                password.Append((char)random.Next(33, 48));
            if (digit)
                password.Append((char)random.Next(48, 58));
            if (lowercase)
                password.Append((char)random.Next(97, 123));
            if (uppercase)
                password.Append((char)random.Next(65, 91));

            return password.ToString();
        }
    }
    }
}
