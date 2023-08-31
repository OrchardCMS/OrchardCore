using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Sms;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers
{
    public class UserInformationDisplayDriver : DisplayDriver<User>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IPhoneFormatValidator _phoneFormatValidator;
        private readonly ISiteService _siteService;
        protected readonly IStringLocalizer S;

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
            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.EditUsers, user))
            {
                return null;
            }

            var site = await _siteService.GetSiteSettingsAsync();
            var settings = site.As<LoginSettings>();
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
            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.EditUsers, user))
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
                if (await context.Updater.TryUpdateModelAsync(userNameModel, Prefix))
                {
                    user.UserName = userNameModel.UserName;
                }

                if (await context.Updater.TryUpdateModelAsync(emailModel, Prefix))
                {
                    user.Email = emailModel.Email;
                }

                if (await context.Updater.TryUpdateModelAsync(phoneNumberModel, Prefix))
                {
                    if (!String.IsNullOrEmpty(phoneNumberModel.PhoneNumber) && !_phoneFormatValidator.IsValid(phoneNumberModel.PhoneNumber))
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(phoneNumberModel.PhoneNumber), S["Please provide a valid phone number."]);
                    }
                    else
                    {
                        user.PhoneNumber = phoneNumberModel.PhoneNumber;
                    }
                }
            }
            else
            {
                var site = await _siteService.GetSiteSettingsAsync();
                var settings = site.As<LoginSettings>();

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

                    if (settings.AllowChangingPhoneNumber && await context.Updater.TryUpdateModelAsync(phoneNumberModel, Prefix))
                    {
                        if (!String.IsNullOrEmpty(phoneNumberModel.PhoneNumber) && !_phoneFormatValidator.IsValid(phoneNumberModel.PhoneNumber))
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

            return await EditAsync(user, context);
        }

        private async Task<bool> CanEditUserInfoAsync(User user)
        {
            return !IsCurrentUser(user) || await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.EditOwnUser);
        }

        private bool IsCurrentUser(User user)
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) == user.UserId;
        }
    }
}
