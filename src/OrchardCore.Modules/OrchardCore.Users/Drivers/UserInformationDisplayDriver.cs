using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers
{
    public class UserInformationDisplayDriver : DisplayDriver<User>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public UserInformationDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override IDisplayResult Edit(User user)
        {
            return Initialize<EditUserInformationViewModel>("UserInformationFields_Edit", async model =>
            {
                model.UserName = user.UserName;
                model.Email = user.Email;
                model.IsEditingDisabled = !await AuthorizeUpdateAsync(user);
            })
            .Location("Content:1")
            .RenderWhen(() => AuthorizeEditAsync(user));
        }

        public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            if (!await AuthorizeUpdateAsync(user))
            {
                return Edit(user);
            }

            var model = new EditUserInformationViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                // Do not use the user manager to set these values, or validate them here, as they will validate at the incorrect time.
                // After this driver runs the IUserService.UpdateAsync or IUserService.CreateAsync method will
                // validate the user and provide the correct error messages based on the entire user objects values.

                // Custom properties should still be validated in the driver.

                user.UserName = model.UserName;
                user.Email = model.Email;
            }

            return Edit(user);
        }

        private Task<bool> AuthorizeUpdateAsync(User user)
        {
            // When the current user matches this user we can ask for ManageOwnUserInformation
            if (String.Equals(user.UserId, _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), StringComparison.OrdinalIgnoreCase))
            {
                return _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageOwnUserInformation);
            }

            // Otherwise we require permission to manage this users information.
            return _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageUsers, user);
        }

        private Task<bool> AuthorizeEditAsync(User user)
        {
            // When the current user matches this user we can ask for ManageOwnUserInformation
            if (String.Equals(user.UserId, _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), StringComparison.OrdinalIgnoreCase))
            {
                return _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageOwnUserInformation);
            }

            // Otherwise we require permission to manage this users information.
            return _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ViewUsers, user);
        }
    }
}
