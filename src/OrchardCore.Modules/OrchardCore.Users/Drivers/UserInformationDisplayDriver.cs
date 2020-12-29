using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers
{
    public class UserInformationDisplayDriver : DisplayDriver<User>
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer S;

        public UserInformationDisplayDriver(
            UserManager<IUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IStringLocalizer<UserInformationDisplayDriver> stringLocalizer)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            S = stringLocalizer;
        }

        public override IDisplayResult Edit(User user)
        {
            return Initialize<EditUserInformationViewModel>("UserInformationFields_Edit", model =>
            {
                model.UserName = user.UserName;
                model.Email = user.Email;
            })
            .Location("Content:1")
            .RenderWhen(() => AuthorizeAsync(user));
        }

        public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            if (!await AuthorizeAsync(user))
            {
                return Edit(user);
            }

            var model = new EditUserInformationViewModel();

            if (!await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                return Edit(user);
            }

            model.UserName = model.UserName?.Trim();
            model.Email = model.Email?.Trim();

            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                context.Updater.ModelState.AddModelError("UserName", S["A user name is required."]);
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                context.Updater.ModelState.AddModelError("Email", S["An email is required."]);
            }

            var userWithSameName = await _userManager.FindByNameAsync(model.UserName);
            if (userWithSameName != null)
            {
                var userWithSameNameId = await _userManager.GetUserIdAsync(userWithSameName);
                if (userWithSameNameId != user.UserId)
                {
                    context.Updater.ModelState.AddModelError(string.Empty, S["The user name is already used."]);
                }
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userWithSameEmail != null)
            {
                var userWithSameEmailId = await _userManager.GetUserIdAsync(userWithSameEmail);
                if (userWithSameEmailId != user.UserId)
                {
                    context.Updater.ModelState.AddModelError(string.Empty, S["The email is already used."]);
                }
            }

            if (context.Updater.ModelState.IsValid)
            {
                user.UserName = model.UserName;
                user.Email = model.Email;
            }

            return Edit(user);
        }

        private async Task<bool> AuthorizeAsync(User user)
        {
            // When the current user matches this user we can ask for ManageOwnUserInformation
            if (String.Equals(user.UserId, _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), StringComparison.OrdinalIgnoreCase))
            {
                return await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageOwnUserInformation);
            }

            // Otherwise we require permission to manage this users information.
            return await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageUsers, user);
        }
    }
}
