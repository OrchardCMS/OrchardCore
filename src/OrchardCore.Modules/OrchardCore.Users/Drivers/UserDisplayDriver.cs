using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class UserDisplayDriver : DisplayDriver<User>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public UserDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IHtmlLocalizer<UserDisplayDriver> htmlLocalizer,
        IStringLocalizer<UserDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(User user, BuildDisplayContext context)
    {
        return CombineAsync(
            Initialize<SummaryAdminUserViewModel>("UserFields", model => model.User = user).Location("SummaryAdmin", "Header:1"),
            Initialize<SummaryAdminUserViewModel>("UserInfo", model => model.User = user).Location("DetailAdmin", "Content:5"),
            Initialize<SummaryAdminUserViewModel>("UserButtons", model => model.User = user).Location("SummaryAdmin", "Actions:1"),
            Initialize<SummaryAdminUserViewModel>("UserActionsMenu", model => model.User = user).Location("SummaryAdmin", "ActionsMenu:5")
        );
    }

    public override async Task<IDisplayResult> EditAsync(User user, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UsersPermissions.EditUsers, user).ConfigureAwait(false))
        {
            return null;
        }

        return Initialize<EditUserViewModel>("UserFields_Edit", model =>
        {
            model.IsNewRequest = context.IsNew;
        })
       .Location("Content:1.5");
    }

    public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
    {
        // To prevent html injection when updating the user must meet all authorization requirements.
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UsersPermissions.EditUsers, user).ConfigureAwait(false))
        {
            // When the user is only editing their profile never update this part of the user.
            return await EditAsync(user, context).ConfigureAwait(false);
        }

        var model = new EditUserViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix).ConfigureAwait(false);

        if (context.IsNew)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Password), S["A password is required"]);
            }

            if (model.Password != model.PasswordConfirmation)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Password), S["The password and the password confirmation fields must match."]);
            }
        }

        if (!context.Updater.ModelState.IsValid)
        {
            return await EditAsync(user, context).ConfigureAwait(false);
        }

        return await EditAsync(user, context).ConfigureAwait(false);
    }
}
