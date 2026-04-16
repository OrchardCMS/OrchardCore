using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class UserPhoneNumberDisplayDriver : DisplayDriver<User>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IPhoneFormatValidator _phoneFormatValidator;
    private readonly ISiteService _siteService;
    private readonly IShellFeaturesManager _shellFeaturesManager;

    internal readonly IStringLocalizer S;

    public UserPhoneNumberDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IPhoneFormatValidator phoneFormatValidator,
        IStringLocalizer<UserPhoneNumberDisplayDriver> stringLocalizer,
        ISiteService siteService,
        IShellFeaturesManager shellFeaturesManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _phoneFormatValidator = phoneFormatValidator;
        S = stringLocalizer;
        _siteService = siteService;
        _shellFeaturesManager = shellFeaturesManager;
    }

    public override async Task<IDisplayResult> EditAsync(User user, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UsersPermissions.EditUsers, user))
        {
            return null;
        }

        var settings = await _siteService.GetSettingsAsync<LoginSettings>();
        var canEditUserInfo = await CanEditUserInfoAsync(user);
        var required = await IsPhoneRequiredAsync();

        return Initialize<EditUserPhoneNumberViewModel>("UserPhoneNumber_Edit", model =>
        {
            model.PhoneNumber = user.PhoneNumber;
            model.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
            model.AllowEditing = context.IsNew || (settings.AllowChangingPhoneNumber && canEditUserInfo);
            model.Required = required;
        }).Location("Content:1.3");
    }

    public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UsersPermissions.EditUsers, user))
        {
            return null;
        }

        var phoneNumberModel = new EditUserPhoneNumberViewModel();
        var required = await IsPhoneRequiredAsync();

        if (context.IsNew)
        {
            await context.Updater.TryUpdateModelAsync(phoneNumberModel, Prefix);

            if (!string.IsNullOrEmpty(phoneNumberModel.PhoneNumber))
            {
                var defaultRegion = !string.IsNullOrEmpty(phoneNumberModel.RegionCode)
                    ? phoneNumberModel.RegionCode
                    : new RegionInfo(Thread.CurrentThread.CurrentUICulture.Name).TwoLetterISORegionName;
                var result = _phoneFormatValidator.Validate(phoneNumberModel.PhoneNumber, defaultRegion);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(phoneNumberModel.PhoneNumber), error.Message);
                    }
                }
                else
                {
                    user.PhoneNumber = result.Value.E164Number;
                }
            }
            else if (required)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(phoneNumberModel.PhoneNumber), S["A phone number is required."]);
            }
            else
            {
                user.PhoneNumber = phoneNumberModel.PhoneNumber;
            }
        }
        else
        {
            var settings = await _siteService.GetSettingsAsync<LoginSettings>();

            if (await CanEditUserInfoAsync(user))
            {
                if (settings.AllowChangingPhoneNumber && await context.Updater.TryUpdateModelAsync(phoneNumberModel, Prefix))
                {
                    if (!string.IsNullOrEmpty(phoneNumberModel.PhoneNumber))
                    {
                        var defaultRegion = !string.IsNullOrEmpty(phoneNumberModel.RegionCode)
                            ? phoneNumberModel.RegionCode
                            : new RegionInfo(Thread.CurrentThread.CurrentUICulture.Name).TwoLetterISORegionName;
                        var result = _phoneFormatValidator.Validate(phoneNumberModel.PhoneNumber, defaultRegion);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                context.Updater.ModelState.AddModelError(Prefix, nameof(phoneNumberModel.PhoneNumber), error.Message);
                            }
                        }
                        else
                        {
                            user.PhoneNumber = result.Value.E164Number;
                        }
                    }
                    else if (required)
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(phoneNumberModel.PhoneNumber), S["A phone number is required."]);
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

    private async Task<bool> IsPhoneRequiredAsync()
        => await _shellFeaturesManager.IsFeatureEnabledAsync(UserConstants.Features.SmsAuthenticator);

    private async Task<bool> CanEditUserInfoAsync(User user)
    {
        return !IsCurrentUser(user) || await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UsersPermissions.EditOwnUser);
    }

    private bool IsCurrentUser(User user)
    {
        return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) == user.UserId;
    }
}
