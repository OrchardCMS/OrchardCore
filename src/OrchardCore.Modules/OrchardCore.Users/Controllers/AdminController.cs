using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Security.Services;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Services;

namespace OrchardCore.Users.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IDisplayManager<UserIndexOptions> _userOptionsDisplayManager;
        private readonly SignInManager<IUser> _signInManager;
        private readonly ISession _session;
        private readonly IAuthorizationService _authorizationService;
        private readonly PagerOptions _pagerOptions;
        private readonly IDisplayManager<User> _userDisplayManager;
        private readonly INotifier _notifier;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IUsersAdminListQueryService _usersAdminListQueryService;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IShapeFactory _shapeFactory;
        private readonly ILogger _logger;

        private readonly dynamic New;
        private readonly IHtmlLocalizer H;
        private readonly IStringLocalizer S;

        public AdminController(
            IDisplayManager<User> userDisplayManager,
            IDisplayManager<UserIndexOptions> userOptionsDisplayManager,
            SignInManager<IUser> signInManager,
            IAuthorizationService authorizationService,
            ISession session,
            UserManager<IUser> userManager,
            IUserService userService,
            IRoleService roleService,
            IUsersAdminListQueryService usersAdminListQueryService,
            INotifier notifier,
            IOptions<PagerOptions> pagerOptions,
            IShapeFactory shapeFactory,
            ILogger<AccountController> logger,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            IStringLocalizer<AdminController> stringLocalizer,
            IUpdateModelAccessor updateModelAccessor)
        {
            _userDisplayManager = userDisplayManager;
            _userOptionsDisplayManager = userOptionsDisplayManager;
            _signInManager = signInManager;
            _authorizationService = authorizationService;
            _session = session;
            _userManager = userManager;
            _notifier = notifier;
            _pagerOptions = pagerOptions.Value;
            _userService = userService;
            _roleService = roleService;
            _usersAdminListQueryService = usersAdminListQueryService;
            _updateModelAccessor = updateModelAccessor;
            _shapeFactory = shapeFactory;
            _logger = logger;

            New = shapeFactory;
            H = htmlLocalizer;
            S = stringLocalizer;
        }

        public async Task<ActionResult> Index([ModelBinder(BinderType = typeof(UserFilterEngineModelBinder), Name = "q")] QueryFilterResult<User> queryFilterResult, PagerParameters pagerParameters)
        {
            // Check a dummy user account to see if the current user has permission to view users.
            var authUser = new User();

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewUsers, authUser))
            {
                return Forbid();
            }

            var options = new UserIndexOptions();

            // Populate route values to maintain previous route data when generating page links
            // await _userOptionsDisplayManager.UpdateEditorAsync(options, _updateModelAccessor.ModelUpdater, false);
            options.FilterResult = queryFilterResult;
            options.FilterResult.MapTo(options);

            // With the options populated we filter the query, allowing the filters to alter the options.
            var users = await _usersAdminListQueryService.QueryAsync(options, _updateModelAccessor.ModelUpdater);

            // The search text is provided back to the UI.
            options.SearchText = options.FilterResult.ToString();
            options.OriginalSearchText = options.SearchText;

            // Populate route values to maintain previous route data when generating page links.
            options.RouteValues.TryAdd("q", options.FilterResult.ToString());

            var routeData = new RouteData(options.RouteValues);

            var pager = new Pager(pagerParameters, _pagerOptions.PageSize);

            var count = await users.CountAsync();

            var results = await users
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ListAsync();

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

            var userEntries = new List<UserEntry>();

            foreach (var user in results)
            {
                userEntries.Add(new UserEntry
                {
                    UserId = user.UserId,
                    Shape = await _userDisplayManager.BuildDisplayAsync(user, updater: _updateModelAccessor.ModelUpdater, displayType: "SummaryAdmin")
                }
                );
            }

            options.UserFilters = new List<SelectListItem>()
            {
                new SelectListItem() { Text = S["All Users"], Value = nameof(UsersFilter.All), Selected = (options.Filter == UsersFilter.All) },
                new SelectListItem() { Text = S["Enabled Users"], Value = nameof(UsersFilter.Enabled), Selected = (options.Filter == UsersFilter.Enabled) },
                new SelectListItem() { Text = S["Disabled Users"], Value = nameof(UsersFilter.Disabled), Selected = (options.Filter == UsersFilter.Disabled) }
                //new SelectListItem() { Text = S["Approved"], Value = nameof(UsersFilter.Approved) },
                //new SelectListItem() { Text = S["Email pending"], Value = nameof(UsersFilter.EmailPending) },
                //new SelectListItem() { Text = S["Pending"], Value = nameof(UsersFilter.Pending) }
            };

            options.UserSorts = new List<SelectListItem>()
            {
                new SelectListItem() { Text = S["Name"], Value = nameof(UsersOrder.Name), Selected = (options.Order == UsersOrder.Name) },
                new SelectListItem() { Text = S["Email"], Value = nameof(UsersOrder.Email), Selected = (options.Order == UsersOrder.Email) },
                //new SelectListItem() { Text = S["Created date"], Value = nameof(UsersOrder.CreatedUtc) },
                //new SelectListItem() { Text = S["Last Login date"], Value = nameof(UsersOrder.LastLoginUtc) }
            };

            options.UsersBulkAction = new List<SelectListItem>()
            {
                new SelectListItem() { Text = S["Approve"], Value = nameof(UsersBulkAction.Approve) },
                new SelectListItem() { Text = S["Enable"], Value = nameof(UsersBulkAction.Enable) },
                new SelectListItem() { Text = S["Disable"], Value = nameof(UsersBulkAction.Disable) },
                new SelectListItem() { Text = S["Delete"], Value = nameof(UsersBulkAction.Delete) }
            };

            var allRoles = (await _roleService.GetRoleNamesAsync())
                .Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase);

            options.UserRoleFilters = new List<SelectListItem>()
            {
                new SelectListItem() { Text = S["All roles"], Value = String.Empty, Selected = (options.SelectedRole == String.Empty) },
                new SelectListItem() { Text = S["Authenticated (no roles)"], Value = "Authenticated", Selected = (String.Equals(options.SelectedRole, "Authenticated", StringComparison.OrdinalIgnoreCase)) }
            };

            // TODO Candidate for dynamic localization.
            options.UserRoleFilters.AddRange(allRoles.Select(x => new SelectListItem { Text = x, Value = x, Selected = (String.Equals(options.SelectedRole, x, StringComparison.OrdinalIgnoreCase)) }));

            // Populate options pager summary values.
            var startIndex = (pagerShape.Page - 1) * (pagerShape.PageSize) + 1;
            options.StartIndex = startIndex;
            options.EndIndex = startIndex + userEntries.Count - 1;
            options.UsersCount = userEntries.Count;
            options.TotalItemCount = pagerShape.TotalItemCount;

            var header = await _userOptionsDisplayManager.BuildEditorAsync(options, _updateModelAccessor.ModelUpdater, false);

            var shapeViewModel = await _shapeFactory.CreateAsync<UsersIndexViewModel>("UsersAdminList", viewModel =>
            {
                viewModel.Users = userEntries;
                viewModel.Pager = pagerShape;
                viewModel.Options = options;
                viewModel.Header = header;
            });

            return View(shapeViewModel);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.Filter")]
        public async Task<ActionResult> IndexFilterPOST(UserIndexOptions options)
        {
            // When the user has typed something into the search input no further evaluation of the form post is required.
            if (!String.Equals(options.SearchText, options.OriginalSearchText, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Index), new RouteValueDictionary { { "q", options.SearchText } });
            }

            // Evaluate the values provided in the form post and map them to the filter result and route values.
            await _userOptionsDisplayManager.UpdateEditorAsync(options, _updateModelAccessor.ModelUpdater, false);

            // The route value must always be added after the editors have updated the models.
            options.RouteValues.TryAdd("q", options.FilterResult.ToString());

            return RedirectToAction(nameof(Index), options.RouteValues);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.BulkAction")]
        public async Task<ActionResult> IndexPOST(UserIndexOptions options, IEnumerable<string> itemIds)
        {
            // Check a dummy user account to see if the current user has permission to manage it.
            var authUser = new User();

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers, authUser))
            {
                return Forbid();
            }

            if (itemIds?.Count() > 0)
            {
                var checkedUsers = await _session.Query<User, UserIndex>().Where(x => x.UserId.IsIn(itemIds)).ListAsync();

                // Bulk actions require the ManageUsers permission on all the checked users.
                // To prevent html injection we authorize each user before performing any operations.
                foreach (var user in checkedUsers)
                {
                    if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers, user))
                    {
                        return Forbid();
                    }
                }

                switch (options.BulkAction)
                {
                    case UsersBulkAction.None:
                        break;
                    case UsersBulkAction.Approve:
                        foreach (var user in checkedUsers)
                        {
                            if (!await _userManager.IsEmailConfirmedAsync(user))
                            {
                                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                                await _userManager.ConfirmEmailAsync(user, token);
                                await _notifier.SuccessAsync(H["User {0} successfully approved.", user.UserName]);
                            }
                        }
                        break;
                    case UsersBulkAction.Delete:
                        foreach (var user in checkedUsers)
                        {
                            if (String.Equals(user.UserId, User.FindFirstValue(ClaimTypes.NameIdentifier), StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            await _userManager.DeleteAsync(user);
                            await _notifier.SuccessAsync(H["User {0} successfully deleted.", user.UserName]);
                        }
                        break;
                    case UsersBulkAction.Disable:
                        foreach (var user in checkedUsers)
                        {
                            if (String.Equals(user.UserId, User.FindFirstValue(ClaimTypes.NameIdentifier), StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            user.IsEnabled = false;
                            await _userManager.UpdateAsync(user);
                            await _notifier.SuccessAsync(H["User {0} successfully disabled.", user.UserName]);
                        }
                        break;
                    case UsersBulkAction.Enable:
                        foreach (var user in checkedUsers)
                        {
                            if (String.Equals(user.UserId, User.FindFirstValue(ClaimTypes.NameIdentifier), StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            user.IsEnabled = true;
                            await _userManager.UpdateAsync(user);
                            await _notifier.SuccessAsync(H["User {0} successfully enabled.", user.UserName]);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Create()
        {
            var user = new User();

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewUsers, user))
            {
                return Forbid();
            }

            var shape = await _userDisplayManager.BuildEditorAsync(user, updater: _updateModelAccessor.ModelUpdater, isNew: true);

            return View(shape);
        }

        [HttpPost]
        [ActionName(nameof(Create))]
        public async Task<IActionResult> CreatePost([Bind(Prefix = "User.Password")] string password)
        {
            var user = new User();

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewUsers, user))
            {
                return Forbid();
            }

            var shape = await _userDisplayManager.UpdateEditorAsync(user, updater: _updateModelAccessor.ModelUpdater, isNew: true);

            if (!ModelState.IsValid)
            {
                return View(shape);
            }

            await _userService.CreateUserAsync(user, password, ModelState.AddModelError);

            if (!ModelState.IsValid)
            {
                return View(shape);
            }

            await _notifier.SuccessAsync(H["User created successfully."]);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id, string returnUrl)
        {
            // When no id is provided we assume the user is trying to edit their own profile.
            var editingOwnUser = false;
            if (String.IsNullOrEmpty(id))
            {
                id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnUserInformation))
                {
                    return Forbid();
                }
                editingOwnUser = true;
            }

            var user = await _userManager.FindByIdAsync(id) as User;
            if (user == null)
            {
                return NotFound();
            }

            if (!editingOwnUser && !await _authorizationService.AuthorizeAsync(User, Permissions.ViewUsers, user))
            {
                return Forbid();
            }

            var shape = await _userDisplayManager.BuildEditorAsync(user, updater: _updateModelAccessor.ModelUpdater, isNew: false);

            ViewData["ReturnUrl"] = returnUrl;

            return View(shape);
        }

        [HttpPost]
        [ActionName(nameof(Edit))]
        public async Task<IActionResult> EditPost(string id, string returnUrl)
        {
            // When no id is provided we assume the user is trying to edit their own profile.
            var editingOwnUser = false;
            if (String.IsNullOrEmpty(id))
            {
                editingOwnUser = true;
                id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnUserInformation))
                {
                    return Forbid();
                }
            }

            var user = await _userManager.FindByIdAsync(id) as User;
            if (user == null)
            {
                return NotFound();
            }

            if (!editingOwnUser && !await _authorizationService.AuthorizeAsync(User, Permissions.ViewUsers, user))
            {
                return Forbid();
            }

            var shape = await _userDisplayManager.UpdateEditorAsync(user, updater: _updateModelAccessor.ModelUpdater, isNew: false);

            if (!ModelState.IsValid)
            {
                return View(shape);
            }

            var result = await _userManager.UpdateAsync(user);

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            if (!ModelState.IsValid)
            {
                return View(shape);
            }

            if (String.Equals(User.FindFirstValue(ClaimTypes.NameIdentifier), user.UserId, StringComparison.OrdinalIgnoreCase))
            {
                await _signInManager.RefreshSignInAsync(user);
            }

            await _notifier.SuccessAsync(H["User updated successfully."]);

            if (editingOwnUser)
            {
                if (!String.IsNullOrEmpty(returnUrl))
                {
                    return this.LocalRedirect(returnUrl, true);
                }

                return RedirectToAction(nameof(Edit));
            }
            else
            {
                if (!String.IsNullOrEmpty(returnUrl))
                {
                    return this.LocalRedirect(returnUrl, true);
                }

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id) as User;

            if (user == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers, user))
            {
                return Forbid();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                await _notifier.SuccessAsync(H["User deleted successfully."]);
            }
            else
            {
                await _session.CancelAsync();

                await _notifier.ErrorAsync(H["Could not delete the user."]);

                foreach (var error in result.Errors)
                {
                    await _notifier.ErrorAsync(H[error.Description]);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id) as User;

            if (user == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers, user))
            {
                return Forbid();
            }

            var model = new ResetPasswordViewModel { Email = user.Email };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditPassword(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email) as User;

            if (user == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers, user))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                if (await _userService.ResetPasswordAsync(model.Email, token, model.NewPassword, ModelState.AddModelError))
                {
                    await _notifier.SuccessAsync(H["Password updated correctly."]);

                    return RedirectToAction(nameof(Index));
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Unlock(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers))
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(id) as User;

            if (user == null)
            {
                return NotFound();
            }

            await _userManager.ResetAccessFailedCountAsync(user);
            var result = await _userManager.SetLockoutEndDateAsync(user, null);

            if (result.Succeeded)
            {
                await _notifier.SuccessAsync(H["User unlocked successfully."]);
            }
            else
            {
                await _session.CancelAsync();

                await _notifier.ErrorAsync(H["Could not unlock the user."]);

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning(error.Description);
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
