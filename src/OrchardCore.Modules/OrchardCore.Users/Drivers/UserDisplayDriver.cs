using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers
{
    public class UserDisplayDriver : DisplayDriver<User>
    {
        private const string AdministratorRole = "Administrator";
        private readonly UserManager<IUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumerable<IUserEventHandler> _userEventHandlers;
        private readonly ILogger _logger;
        protected readonly IHtmlLocalizer H;
        protected readonly IStringLocalizer S;

        public UserDisplayDriver(
            UserManager<IUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            ILogger<UserDisplayDriver> logger,
            IEnumerable<IUserEventHandler> userEventHandlers,
            IAuthorizationService authorizationService,
            IHtmlLocalizer<UserDisplayDriver> htmlLocalizer,
            IStringLocalizer<UserDisplayDriver> stringLocalizer)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            _authorizationService = authorizationService;
            _logger = logger;
            _userEventHandlers = userEventHandlers;
            H = htmlLocalizer;
            S = stringLocalizer;
        }

        public override IDisplayResult Display(User user)
        {
            return Combine(
                Initialize<SummaryAdminUserViewModel>("UserFields", model => model.User = user).Location("SummaryAdmin", "Header:1"),
                Initialize<SummaryAdminUserViewModel>("UserInfo", model => model.User = user).Location("DetailAdmin", "Content:5"),
                Initialize<SummaryAdminUserViewModel>("UserButtons", model => model.User = user).Location("SummaryAdmin", "Actions:1")
            );
        }

        public override async Task<IDisplayResult> EditAsync(User user, BuildEditorContext context)
        {
            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.EditUsers, user))
            {
                return null;
            }

            return Initialize<EditUserViewModel>("UserFields_Edit", model =>
            {
                model.EmailConfirmed = user.EmailConfirmed;
                model.IsEnabled = user.IsEnabled;
                model.IsNewRequest = context.IsNew;
                // The current user cannot disable themselves, nor can a user without permission to manage this user disable them.
                model.IsEditingDisabled = IsCurrentUser(user);
            })
           .Location("Content:1.5");
        }

        public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            // To prevent html injection when updating the user must meet all authorization requirements.
            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.EditUsers, user))
            {
                // When the user is only editing their profile never update this part of the user.
                return await EditAsync(user, context);
            }

            var model = new EditUserViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            if (context.IsNew)
            {
                if (String.IsNullOrWhiteSpace(model.Password))
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
                return await EditAsync(user, context);
            }

            var isEditingDisabled = IsCurrentUser(user);

            if (!isEditingDisabled && !model.IsEnabled && user.IsEnabled)
            {
                var enabledUsersOfAdminRole = (await _userManager.GetUsersInRoleAsync(AdministratorRole))
                    .Cast<User>()
                    .Where(user => user.IsEnabled)
                    .ToList();

                if (enabledUsersOfAdminRole.Count == 1 && user.UserId == enabledUsersOfAdminRole.First().UserId)
                {
                    await _notifier.WarningAsync(H["Cannot disable the only enabled administrator."]);
                }
                else
                {
                    user.IsEnabled = model.IsEnabled;
                    var userContext = new UserContext(user);
                    // TODO This handler should be invoked through the create or update methods.
                    // otherwise it will not be invoked when a workflow, or other operation, changes this value.
                    await _userEventHandlers.InvokeAsync((handler, context) => handler.DisabledAsync(userContext), userContext, _logger);
                }
            }
            else if (!isEditingDisabled && model.IsEnabled && !user.IsEnabled)
            {
                user.IsEnabled = model.IsEnabled;
                var userContext = new UserContext(user);
                // TODO This handler should be invoked through the create or update methods.
                // otherwise it will not be invoked when a workflow, or other operation, changes this value.
                await _userEventHandlers.InvokeAsync((handler, context) => handler.EnabledAsync(userContext), userContext, _logger);
            }

            if (context.Updater.ModelState.IsValid)
            {
                if (model.EmailConfirmed && !await _userManager.IsEmailConfirmedAsync(user))
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);
                }
            }

            return await EditAsync(user, context);
        }

        private bool IsCurrentUser(User user)
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) == user.UserId;
        }
    }
}
