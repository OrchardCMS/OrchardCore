using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.Security.Services;
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
        private IEnumerable<IUserEventHandler> _userEventHandlers;
        private readonly ILogger _logger;
        private readonly IHtmlLocalizer H;


        public UserDisplayDriver(
            UserManager<IUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            ILogger<UserDisplayDriver> logger,
            IEnumerable<IUserEventHandler> userEventHandlers,
            IAuthorizationService authorizationService,
            IHtmlLocalizer<UserDisplayDriver> htmlLocalizer)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            _authorizationService = authorizationService;
            _logger = logger;
            _userEventHandlers = userEventHandlers;
            H = htmlLocalizer;
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
            return Task.FromResult<IDisplayResult>(Initialize<EditUserViewModel>("UserFields_Edit", model =>
            {
                model.EmailConfirmed = user.EmailConfirmed;
                model.IsEnabled = user.IsEnabled;
            })
            .Location("Content:1.5")
            .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageUsers)));
        }

        public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageUsers))
            {
                // When the user is only editing their profile never update this part of the user.
                return await EditAsync(user, context);
            }

            var model = new EditUserViewModel();
            var httpContext = _httpContextAccessor.HttpContext;

            if (!await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                return await EditAsync(user, context);
            }

            if (!model.IsEnabled && user.IsEnabled)
            {
                var usersOfAdminRole = (await _userManager.GetUsersInRoleAsync(AdministratorRole)).Cast<User>();;
                if (usersOfAdminRole.Count() == 1 && String.Equals(user.UserId, usersOfAdminRole.First().UserId , StringComparison.OrdinalIgnoreCase))
                {
                    _notifier.Warning(H["Cannot disable the only administrator."]);
                }
                else
                {
                    if (user.UserId != httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier))
                    {
                        user.IsEnabled = model.IsEnabled;
                        var userContext = new UserContext(user);
                        await _userEventHandlers.InvokeAsync((handler, context) => handler.DisabledAsync(userContext), userContext, _logger);
                    }
                    else
                    {
                        _notifier.Warning(H["Cannot disable current user."]);
                    }
                }
            }
            else if (model.IsEnabled && !user.IsEnabled)
            {
                user.IsEnabled = model.IsEnabled;
                var userContext = new UserContext(user);
                await _userEventHandlers.InvokeAsync((handler, context) => handler.EnabledAsync(userContext), userContext, _logger);
            }

            if (context.Updater.ModelState.IsValid)
            {
                if (model.EmailConfirmed)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);
                }
            }

            return await EditAsync(user, context);
        }
    }
}
