using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Sms;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class UserInformationDisplayDriver : DisplayDriver<User>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IPhoneFormatValidator _phoneFormatValidator;
    private readonly ISiteService _siteService;

    internal readonly IStringLocalizer S;

    public UserInformationDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IPhoneFormatValidator phoneFormatValidator,
        IStringLocalizer<UserInformationDisplayDriver> stringLocalizer,
        ISiteService siteService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _phoneFormatValidator = phoneFormatValidator;
        S = stringLocalizer;
        _siteService = siteService;
    }

    public override async Task<IDisplayResult> EditAsync(User user, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UsersPermissions.EditUsers, user).ConfigureAwait(false))
        {
            return null;
        }

        var settings = await _siteService.GetSettingsAsync<LoginSettings>().ConfigureAwait(false);
        var canEditUserInfo = await CanEditUserInfoAsync(user).ConfigureAwait(false);
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

            }).Location("Content:1.3"),

            Initialize<EditUserPhoneNumberViewModel>("UserPhoneNumber_Edit", model =>
            {
                model.PhoneNumber = user.PhoneNumber;
                model.PhoneNumberConfirmed = user.PhoneNumberConfirmed;

                model.AllowEditing = context.IsNew || (settings.AllowChangingPhoneNumber && canEditUserInfo);

            }).Location("Content:1.3")
        );
    }

    public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UsersPermissions.EditUsers, user).ConfigureAwait(false))
        {
            return null;
        }

        var userNameModel = new EditUserNameViewModel();
        var emailModel = new EditUserEmailViewModel();
        var phoneNumberModel = new EditUserPhoneNumberViewModel();

        // Do not use the user manager to set these values, or validate them here, as they will validate at the incorrect time.
        // After this driver runs the IUserService.UpdateAsync or IUserService.CreateAsync method will
        // validate the user and provide the correct error messages based on the entire user objects values.

        // Custom properties should still be validated in the driver.

        if (context.IsNew)
        {
            await context.Updater.TryUpdateModelAsync(userNameModel, Prefix).ConfigureAwait(false);

            user.UserName = userNameModel.UserName;

            await context.Updater.TryUpdateModelAsync(emailModel, Prefix).ConfigureAwait(false);

            user.Email = emailModel.Email;

            await context.Updater.TryUpdateModelAsync(phoneNumberModel, Prefix).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(phoneNumberModel.PhoneNumber) && !_phoneFormatValidator.IsValid(phoneNumberModel.PhoneNumber))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(phoneNumberModel.PhoneNumber), S["Please provide a valid phone number."]);
            }
            else
            {
                user.PhoneNumber = phoneNumberModel.PhoneNumber;
            }
        }
        else
        {
            var settings = await _siteService.GetSettingsAsync<LoginSettings>().ConfigureAwait(false);

            if (await CanEditUserInfoAsync(user).ConfigureAwait(false))
            {
                if (settings.AllowChangingUsername && await context.Updater.TryUpdateModelAsync(userNameModel, Prefix).ConfigureAwait(false))
                {
                    user.UserName = userNameModel.UserName;
                }

                if (settings.AllowChangingEmail && await context.Updater.TryUpdateModelAsync(emailModel, Prefix).ConfigureAwait(false))
                {
                    user.Email = emailModel.Email;
                }

                if (settings.AllowChangingPhoneNumber && await context.Updater.TryUpdateModelAsync(phoneNumberModel, Prefix).ConfigureAwait(false))
                {
                    if (!string.IsNullOrEmpty(phoneNumberModel.PhoneNumber) && !_phoneFormatValidator.IsValid(phoneNumberModel.PhoneNumber))
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(phoneNumberModel.PhoneNumber), S["Please provide a valid phone number."]);
                    }
                    else
                    {
                        user.PhoneNumber = phoneNumberModel.PhoneNumber;
                    }
                }
            }
        }

        return await EditAsync(user, context).ConfigureAwait(false);
    }

    private async Task<bool> CanEditUserInfoAsync(User user)
    {
        return !IsCurrentUser(user) || await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UsersPermissions.EditOwnUser).ConfigureAwait(false);
    }

    private bool IsCurrentUser(User user)
    {
        return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) == user.UserId;
    }
}
