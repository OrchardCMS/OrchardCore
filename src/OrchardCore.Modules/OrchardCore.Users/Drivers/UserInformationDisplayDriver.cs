using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class UserInformationDisplayDriver : DisplayDriver<User>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISiteService _siteService;

    public UserInformationDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        ISiteService siteService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _siteService = siteService;
    }

    public override async Task<IDisplayResult> EditAsync(User user, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UsersPermissions.EditUsers, user))
        {
            return null;
        }

        var settings = await _siteService.GetSettingsAsync<LoginSettings>();
        var canEditUserInfo = await CanEditUserInfoAsync(user);

        return Combine(
            Initialize<EditUserNameViewModel>("UserName_Edit", model =>
            {
                model.UserName = user.UserName;
                model.AllowEditing = context.IsNew || (settings.AllowChangingUsername && canEditUserInfo);
            }).Location("Content:1"),

            Initialize<EditUserEmailViewModel>("UserEmail_Edit", model =>
            {
                model.Email = user.Email;
                model.AllowEditing = context.IsNew || (settings.AllowChangingEmail && canEditUserInfo);
            }).Location("Content:1.3")
        );
    }

    public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UsersPermissions.EditUsers, user))
        {
            return null;
        }

        var userNameModel = new EditUserNameViewModel();
        var emailModel = new EditUserEmailViewModel();

        // Do not use the user manager to set these values, or validate them here, as they will validate at the incorrect time.
        // After this driver runs the IUserService.UpdateAsync or IUserService.CreateAsync method will
        // validate the user and provide the correct error messages based on the entire user objects values.

        if (context.IsNew)
        {
            await context.Updater.TryUpdateModelAsync(userNameModel, Prefix);
            user.UserName = userNameModel.UserName;

            await context.Updater.TryUpdateModelAsync(emailModel, Prefix);
            user.Email = emailModel.Email;
        }
        else
        {
            var settings = await _siteService.GetSettingsAsync<LoginSettings>();

            if (await CanEditUserInfoAsync(user))
            {
                if (settings.AllowChangingUsername && await context.Updater.TryUpdateModelAsync(userNameModel, Prefix))
                {
                    user.UserName = userNameModel.UserName;
                }

                if (settings.AllowChangingEmail && await context.Updater.TryUpdateModelAsync(emailModel, Prefix))
                {
                    user.Email = emailModel.Email;
                }
            }
        }

        return await EditAsync(user, context);
    }

    private async Task<bool> CanEditUserInfoAsync(User user)
    {
        return !IsCurrentUser(user) || await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UsersPermissions.EditOwnUser);
    }

    private bool IsCurrentUser(User user)
    {
        return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) == user.UserId;
    }
}
