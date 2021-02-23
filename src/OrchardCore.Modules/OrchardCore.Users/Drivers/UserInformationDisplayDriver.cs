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
            return Initialize<EditUserInformationViewModel>("UserInformationFields_Edit", model =>
            {
                model.UserName = user.UserName;
                model.Email = user.Email;
            })
            .Location("Content:1")
            .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageOwnUserInformation));
        }

        public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            var model = new EditUserInformationViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                // Do not user the user manager to set these values, as they will validate at the incorrect time.
                // After this driver runs the IUserService.UpdateAsync or IUserService.CreateAsync method will
                // validate the user and provide the correct error messages based on the entire user objects values.

                // Custom properties should still be validated in the driver.

                user.UserName = model.UserName;
                user.Email = model.Email;
            }

            return Edit(user);
        }
    }
}
