using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IRoleService _roleService;
        private readonly IUserRoleStore<IUser> _userRoleStore;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger _logger;
        private readonly IHtmlLocalizer H;

        public UserDisplayDriver(
            UserManager<IUser> userManager,
            IRoleService roleService,
            IUserRoleStore<IUser> userRoleStore,
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            ILogger<UserDisplayDriver> logger,
            IEnumerable<IUserEventHandler> handlers,
            IAuthorizationService authorizationService,
            IHtmlLocalizer<UserDisplayDriver> htmlLocalizer)
        {
            _userManager = userManager;
            _roleService = roleService;
            _userRoleStore = userRoleStore;
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            _authorizationService = authorizationService;
            _logger = logger;
            Handlers = handlers;
            H = htmlLocalizer;
        }

        public IEnumerable<IUserEventHandler> Handlers { get; private set; }

        public override IDisplayResult Display(User user)
        {
            return Combine(
                Initialize<SummaryAdminUserViewModel>("UserFields", model => model.User = user).Location("SummaryAdmin", "Header:1"),
                Initialize<SummaryAdminUserViewModel>("UserButtons", model => model.User = user).Location("SummaryAdmin", "Actions:1")
            );
        }

        public override Task<IDisplayResult> EditAsync(User user, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(Initialize<EditUserViewModel>("UserFields_Edit", async model =>
            {
                var roleNames = await GetRoleNamesAsync();
                var userRoleNames = await _userManager.GetRolesAsync(user);
                var roles = roleNames.Select(x => new RoleViewModel { Role = x, IsSelected = userRoleNames.Contains(x, StringComparer.OrdinalIgnoreCase) }).ToArray();

                model.Roles = roles;
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

            var usersOfAdminRole = await _userManager.GetUsersInRoleAsync(AdministratorRole);
            if (!model.IsEnabled && user.IsEnabled)
            {
                if (usersOfAdminRole.Count == 1 && usersOfAdminRole.First().UserName == user.UserName)
                {
                    _notifier.Warning(H["Cannot disable the only administrator."]);
                }
                else
                {
                    if (user.UserName != httpContext.User.Identity.Name)
                    {
                        user.IsEnabled = model.IsEnabled;
                        var userContext = new UserContext(user);
                        // TODO This handler should be invoked through the create or update methods.
                        // otherwise it will not be invoked when a workflow changes this value.
                        // or other operation.
                        await Handlers.InvokeAsync((handler, context) => handler.DisabledAsync(userContext), userContext, _logger);
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
                // TODO This handler should be invoked through the or update methods.
                // otherwise it will not be invoked when a workflow changes this value.
                // or other operation.
                await Handlers.InvokeAsync((handler, context) => handler.EnabledAsync(userContext), userContext, _logger);
            }

            if (context.Updater.ModelState.IsValid)
            {
                if (model.EmailConfirmed)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);
                }

                var roleNames = model.Roles.Where(x => x.IsSelected).Select(x => x.Role).ToList();

                if (context.IsNew)
                {
                    // Add new roles
                    foreach (var role in roleNames)
                    {
                        await _userRoleStore.AddToRoleAsync(user, _userManager.NormalizeName(role), default(CancellationToken));
                    }
                }
                else
                {
                    // Remove roles in two steps to prevent an iteration on a modified collection
                    var rolesToRemove = new List<string>();
                    foreach (var role in await _userRoleStore.GetRolesAsync(user, default(CancellationToken)))
                    {
                        if (!roleNames.Contains(role))
                        {
                            rolesToRemove.Add(role);
                        }
                    }

                    foreach (var role in rolesToRemove)
                    {
                        // Make sure we always have at least one administrator account
                        if (usersOfAdminRole.Count == 1 && usersOfAdminRole.First().UserName == user.UserName && role == AdministratorRole)
                        {
                            _notifier.Warning(H["Cannot remove administrator role from the only administrator."]);
                            continue;
                        }
                        else
                        {
                            await _userRoleStore.RemoveFromRoleAsync(user, _userManager.NormalizeName(role), default(CancellationToken));
                        }
                    }

                    // Add new roles
                    foreach (var role in roleNames)
                    {
                        if (!await _userRoleStore.IsInRoleAsync(user, _userManager.NormalizeName(role), default(CancellationToken)))
                        {
                            await _userRoleStore.AddToRoleAsync(user, _userManager.NormalizeName(role), default(CancellationToken));
                        }
                    }
                }
            }

            return await EditAsync(user, context);
        }

        private async Task<IEnumerable<string>> GetRoleNamesAsync()
        {
            var roleNames = await _roleService.GetRoleNamesAsync();
            return roleNames.Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase);
        }
    }
}
