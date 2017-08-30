using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Notify;
using Orchard.Navigation;
using Orchard.Security;
using Orchard.Security.Services;
using Orchard.Settings;
using Orchard.Users.Indexes;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Users.ViewModels;
using YesSql;

namespace Orchard.Users.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<IUser> _userManager;
        private readonly ISession _session;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer T;
        private readonly IHtmlLocalizer TH;
        private readonly ISiteService _siteService;
        private readonly dynamic New;
        private readonly RoleManager<IRole> _roleManager;
        private readonly IRoleProvider _roleProvider;
        private readonly INotifier _notifier;
        private readonly IUserService _userService;

        public AdminController(
            IAuthorizationService authorizationService,
            ISession session,
            UserManager<IUser> userManager,
            RoleManager<IRole> roleManager,
            IRoleProvider roleProvider,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            INotifier notifier,
            IUserService userService
            )
        {
            _notifier = notifier;
            _roleProvider = roleProvider;
            _roleManager = roleManager;
            New = shapeFactory;
            _siteService = siteService;
            T = stringLocalizer;
            TH = htmlLocalizer;
            _authorizationService = authorizationService;
            _session = session;
            _userManager = userManager;
            _userService = userService;
        }
        public async Task<ActionResult> Index(UserIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            // default options
            if (options == null)
            {
                options = new UserIndexOptions();
            }

            var users = _session.Query<User, UserIndex>();

            switch (options.Filter)
            {
                case UsersFilter.Approved:
                    //users = users.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
                case UsersFilter.Pending:
                    //users = users.Where(u => u.RegistrationStatus == UserStatus.Pending);
                    break;
                case UsersFilter.EmailPending:
                    //users = users.Where(u => u.EmailStatus == UserStatus.Pending);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                users = users.Where(u => u.NormalizedUserName.Contains(options.Search) || u.NormalizedEmail.Contains(options.Search));
            }

            switch (options.Order)
            {
                case UsersOrder.Name:
                    users = users.OrderBy(u => u.NormalizedUserName);
                    break;
                case UsersOrder.Email:
                    users = users.OrderBy(u => u.NormalizedEmail);
                    break;
                case UsersOrder.CreatedUtc:
                    //users = users.OrderBy(u => u.CreatedUtc);
                    break;
                case UsersOrder.LastLoginUtc:
                    //users = users.OrderBy(u => u.LastLoginUtc);
                    break;
            }

            var count = await users.CountAsync();

            var results = await users
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ListAsync();

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            var pagerShape = New.Pager(pager).TotalItemCount(count).RouteData(routeData);

            var model = new UsersIndexViewModel
            {
                Users = results
                    .Select(x => new UserEntry { User = x })
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers))
            {
                return Unauthorized();
            }

            var roleNames = await GetRoleNamesAsync();
            var roles = roleNames.Select(x => new RoleViewModel { Role = x }).ToArray();

            var model = new CreateUserViewModel
            {
                Roles = roles
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            CleanViewModel(model);

            if (ModelState.IsValid)
            {
                var roleNames = model.Roles.Where(x => x.IsSelected).Select(x => x.Role).ToArray();
                var user = await _userService.CreateUserAsync(model.UserName, model.Email, roleNames, model.Password, (key, message) => ModelState.AddModelError(key, message));

                if (user != null)
                {
                    _notifier.Success(TH["User created successfully"]);
                    return RedirectToAction(nameof(Index));
                }

                _session.Cancel();
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers))
            {
                return Unauthorized();
            }

            var currentUser = await _userManager.FindByIdAsync(id);

            if (currentUser == null)
            {
                return NotFound();
            }

            var roleNames = await GetRoleNamesAsync();
            var userRoleNames = await _userManager.GetRolesAsync(currentUser);
            var roles = roleNames.Select(x => new RoleViewModel { Role = x, IsSelected = userRoleNames.Contains(x, StringComparer.OrdinalIgnoreCase) }).ToArray();

            var model = new EditUserViewModel
            {
                Id = id,
                Email = await _userManager.GetEmailAsync(currentUser),
                UserName = await _userManager.GetUserNameAsync(currentUser),
                Roles = roles
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers))
            {
                return Unauthorized();
            }

            CleanViewModel(model);

            var currentUser = await _userManager.FindByIdAsync(model.Id.ToString());

            if (currentUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userWithSameName = await _userManager.FindByNameAsync(model.UserName);
                if (userWithSameName != null)
                {
                    var userWithSameNameId = await _userManager.GetUserIdAsync(userWithSameName);
                    if (userWithSameNameId != model.Id)
                    {
                        ModelState.AddModelError(string.Empty, T["The user name is already used."]);
                    }
                }

                var userWithSameEmail = await _userManager.FindByEmailAsync(model.Email);
                if (userWithSameEmail != null)
                {
                    var userWithSameEmailId = await _userManager.GetUserIdAsync(userWithSameEmail);
                    if (userWithSameEmailId != model.Id)
                    {
                        ModelState.AddModelError(string.Empty, T["The email is already used."]);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var roleNames = model.Roles.Where(x => x.IsSelected).Select(x => x.Role).ToList();
                await _userManager.SetUserNameAsync(currentUser, model.UserName);
                await _userManager.SetEmailAsync(currentUser, model.Email);

                // Remove roles in two steps to prevent an iteration on a modified collection
                var rolesToRemove = new List<string>();
                foreach (var role in await _userManager.GetRolesAsync(currentUser))
                {
                    if (!roleNames.Contains(role))
                    {
                        rolesToRemove.Add(role);
                    }
                }

                foreach(var role in rolesToRemove)
                {
                    await _userManager.RemoveFromRoleAsync(currentUser, role);
                }

                // Add new roles
                foreach (var role in roleNames)
                {
                    if (!await _userManager.IsInRoleAsync(currentUser, role))
                    {
                        await _userManager.AddToRoleAsync(currentUser, role);
                    }
                }

                var result = await _userManager.UpdateAsync(currentUser);

                if (result.Succeeded)
                {
                    _notifier.Success(TH["User updated successfully"]);
                    return RedirectToAction(nameof(Index));
                }

                _session.Cancel();

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers))
            {
                return Unauthorized();
            }

            var currentUser = await _userManager.FindByIdAsync(id);

            if (currentUser == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(currentUser);

            if (result.Succeeded)
            {
                _notifier.Success(TH["User deleted successfully"]);
            }
            else
            {
                _session.Cancel();

                _notifier.Error(TH["Could not delete the user"]);

                foreach (var error in result.Errors)
                {
                    _notifier.Error(TH[error.Description]);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IEnumerable<string>> GetRoleNamesAsync()
        {
            var roleNames = await _roleProvider.GetRoleNamesAsync();
            return roleNames.Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase);
        }

        public void CleanViewModel(CreateUserViewModel model)
        {
            model.UserName = model.UserName?.Trim();
            model.Email = model.Email?.Trim();
        }

        public void CleanViewModel(EditUserViewModel model)
        {
            model.UserName = model.UserName?.Trim();
            model.Email = model.Email?.Trim();
        }
    }
}
