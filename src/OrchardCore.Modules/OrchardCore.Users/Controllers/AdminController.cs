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
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Security.Services;
using OrchardCore.Settings;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;
using YesSql;
using YesSql.Services;
using YesSql.Sql;

namespace OrchardCore.Users.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<IUser> _userManager;
        private readonly SignInManager<IUser> _signInManager;
        private readonly ISession _session;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly IDisplayManager<User> _userDisplayManager;
        private readonly INotifier _notifier;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        private readonly dynamic New;
        private readonly IHtmlLocalizer H;
        private readonly IStringLocalizer S;

        public AdminController(
            IDisplayManager<User> userDisplayManager,
            SignInManager<IUser> signInManager,
            IAuthorizationService authorizationService,
            ISession session,
            UserManager<IUser> userManager,
            IUserService userService,
            IRoleService roleService,
            INotifier notifier,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            IStringLocalizer<AdminController> stringLocalizer,
            IUpdateModelAccessor updateModelAccessor)
        {
            _userDisplayManager = userDisplayManager;
            _signInManager = signInManager;
            _authorizationService = authorizationService;
            _session = session;
            _userManager = userManager;
            _notifier = notifier;
            _siteService = siteService;
            _userService = userService;
            _roleService = roleService;
            _updateModelAccessor = updateModelAccessor;

            New = shapeFactory;
            H = htmlLocalizer;
            S = stringLocalizer;
        }

        public async Task<ActionResult> Index(UserIndexOptions options, PagerParameters pagerParameters)
        {
            // Check a dummy user account to see if the current user has permission to manage it.
            var authUser = new User();

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers, authUser))
            {
                return Forbid();
            }



            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

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
                var normalizedSearchUserName = _userManager.NormalizeName(options.Search);
                var normalizedSearchEMail = _userManager.NormalizeEmail(options.Search);

                users = users.Where(u => u.NormalizedUserName.Contains(normalizedSearchUserName) || u.NormalizedEmail.Contains(normalizedSearchEMail));
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

            var roleNames = await GetNormalizedRoleNamesAsync();
            var authorizedRoleNames = await GetAuthorizedRoleNamesAsync(roleNames);
            var hasManageAuthenticatedPermission = await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsersInAuthenticatedRole);

            // Alter the query if the user does not have permission for all roles.
            if (roleNames.Count() != authorizedRoleNames.Count())
            {
                /*  Filter users and return on users who are in a role which the current user is authorized to manage.
                    If a user is in two roles and the current user can only manage one of those roles they are not authorized to manage the user.

                    Sample code for Sqlite
                    SELECT DISTINCT [Document].*, [UserIndex_a1].[NormalizedUserName]
                    FROM [Document]
                    INNER JOIN [UserIndex] AS [UserIndex_a1] ON [UserIndex_a1].[DocumentId] = [Document].[Id]

                    WHERE not exists (
                        SELECT [RoleName]
                        FROM UserByRoleNameIndex
                        INNER JOIN [UserByRoleNameIndex_Document] ON [UserByRoleNameIndex].[Id] = [UserByRoleNameIndex_Document].[UserByRoleNameIndexId]
                        WHERE [UserByRoleNameIndex_Document].[DocumentId] = [Document].[Id]
                        GROUP BY [RoleName]
                        HAVING [RoleName]  IN ('MODERATOR', 'CONTRIBUTOR', 'AUTHOR', 'ADMINISTRATOR')
                    )

                    ORDER BY [UserIndex_a1].[NormalizedUserName] LIMIT 10 OFFSET 0

                */

                var dialect = _session.Store.Dialect;
                var builderNotExists = dialect.CreateBuilder(_session.Store.Configuration.TablePrefix);
                var indexType = typeof(UserByRoleNameIndex);
                var indexTable = _session.Store.Configuration.TableNameConvention.GetIndexTable(indexType);
                var documentTable = _session.Store.Configuration.TableNameConvention.GetDocumentTable();
                var bridgeTableName = indexTable + "_" + documentTable;
                var bridgeTableIndexColumnName = indexType.Name + "Id";

                builderNotExists.Select();
                builderNotExists.AddSelector(dialect.QuoteForColumnName("RoleName"));
                builderNotExists.From(indexTable);
                builderNotExists.InnerJoin(bridgeTableName, indexTable, "Id", bridgeTableName, bridgeTableIndexColumnName);


                builderNotExists.WhereAnd($"{dialect.QuoteForTableName(bridgeTableName)}.{dialect.QuoteForColumnName("DocumentId")} = {dialect.QuoteForTableName(documentTable)}.{dialect.QuoteForColumnName("Id")}");
                builderNotExists.GroupBy($"{dialect.QuoteForColumnName("RoleName")}");

                var notAuthorizedRolesQueryValue = String.Join(", ", roleNames.Except(authorizedRoleNames).Select(x => dialect.GetSqlValue(x)));
                builderNotExists.Having($"{dialect.QuoteForColumnName("RoleName")} {dialect.InOperator(notAuthorizedRolesQueryValue)}");

                var notExistsCmd = builderNotExists.ToSqlString();

                if (!hasManageAuthenticatedPermission)
                {
                    /* When the user does not have permission to manage authenticated users append this predicate to ensure that users with no roles in the database are not included.

                    Sample code for Sqlite
                        AND EXISTS
                        (   SELECT [RoleName]
                            FROM UserByRoleNameIndex
                            INNER JOIN [UserByRoleNameIndex_Document] ON [UserByRoleNameIndex].[Id] = [UserByRoleNameIndex_Document].[UserByRoleNameIndexId]
                            WHERE [UserByRoleNameIndex_Document].[DocumentId] = [Document].[Id]
                        )
                    */

                    var builderExists = dialect.CreateBuilder(_session.Store.Configuration.TablePrefix);

                    builderExists.Select();
                    builderExists.AddSelector(dialect.QuoteForColumnName("RoleName"));
                    builderExists.From(indexTable);
                    builderExists.InnerJoin(bridgeTableName, indexTable, "Id", bridgeTableName, bridgeTableIndexColumnName);
                    builderExists.WhereAnd($"{dialect.QuoteForTableName(bridgeTableName)}.{dialect.QuoteForColumnName("DocumentId")} = {dialect.QuoteForTableName(documentTable)}.{dialect.QuoteForColumnName("Id")}");

                    var existsCmd = builderExists.ToSqlString();

                    users.Where($"not exists ({notExistsCmd}) and exists ({existsCmd})");
                }
                else
                {
                    users.Where($"not exists ({notExistsCmd})");
                }
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

            var model = new UsersIndexViewModel
            {
                Users = userEntries,
                Options = options,
                Pager = pagerShape
            };

            model.Options.UserFilters = new List<SelectListItem>() {
                new SelectListItem() { Text = S["All"], Value = nameof(UsersFilter.All) },
                //new SelectListItem() { Text = S["Approved"], Value = nameof(UsersFilter.Approved) },
                //new SelectListItem() { Text = S["Email pending"], Value = nameof(UsersFilter.EmailPending) },
                //new SelectListItem() { Text = S["Pending"], Value = nameof(UsersFilter.Pending) }
            };

            model.Options.UserSorts = new List<SelectListItem>() {
                new SelectListItem() { Text = S["Name"], Value = nameof(UsersOrder.Name) },
                new SelectListItem() { Text = S["Email"], Value = nameof(UsersOrder.Email) },
                //new SelectListItem() { Text = S["Created date"], Value = nameof(UsersOrder.CreatedUtc) },
                //new SelectListItem() { Text = S["Last Login date"], Value = nameof(UsersOrder.LastLoginUtc) }
            };

            model.Options.UsersBulkAction = new List<SelectListItem>() {
                new SelectListItem() { Text = S["Approve"], Value = nameof(UsersBulkAction.Approve) },
                new SelectListItem() { Text = S["Enable"], Value = nameof(UsersBulkAction.Enable) },
                new SelectListItem() { Text = S["Disable"], Value = nameof(UsersBulkAction.Disable) },
                new SelectListItem() { Text = S["Delete"], Value = nameof(UsersBulkAction.Delete) }
            };

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.Filter")]
        public ActionResult IndexFilterPOST(UsersIndexViewModel model)
        {
            return RedirectToAction("Index", new RouteValueDictionary {
                { "Options.Filter", model.Options.Filter },
                { "Options.Order", model.Options.Order },
                { "Options.Search", model.Options.Search }
            });
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
                                _notifier.Success(H["User {0} successfully approved.", user.UserName]);
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
                            _notifier.Success(H["User {0} successfully deleted.", user.UserName]);
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
                            _notifier.Success(H["User {0} successfully disabled.", user.UserName]);
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
                            _notifier.Success(H["User {0} successfully enabled.", user.UserName]);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Create()
        {
            var user = new User();

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers, user))
            {
                return Forbid();
            }

            var shape = await _userDisplayManager.BuildEditorAsync(user, updater: _updateModelAccessor.ModelUpdater, isNew: true);

            return View(shape);
        }

        [HttpPost]
        [ActionName(nameof(Create))]
        public async Task<IActionResult> CreatePost()
        {
            var user = new User();

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers, user))
            {
                return Forbid();
            }

            var shape = await _userDisplayManager.UpdateEditorAsync(user, updater: _updateModelAccessor.ModelUpdater, isNew: true);

            if (!ModelState.IsValid)
            {
                return View(shape);
            }

            await _userService.CreateUserAsync(user, null, ModelState.AddModelError);

            if (!ModelState.IsValid)
            {
                return View(shape);
            }

            _notifier.Success(H["User created successfully."]);

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

            if (!editingOwnUser && !await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers, user))
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

            if (!editingOwnUser && !await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers, user))
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

            _notifier.Success(H["User updated successfully."]);

            if (editingOwnUser)
            {
                if (!String.IsNullOrEmpty(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction(nameof(Edit));
            }
            else
            {
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
                _notifier.Success(H["User deleted successfully."]);
            }
            else
            {
                _session.Cancel();

                _notifier.Error(H["Could not delete the user."]);

                foreach (var error in result.Errors)
                {
                    _notifier.Error(H[error.Description]);
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
                    _notifier.Success(H["Password updated correctly."]);

                    return RedirectToAction(nameof(Index));
                }
            }

            return View(model);
        }

        private async Task<IEnumerable<string>> GetNormalizedRoleNamesAsync()
        {
            var roleNames = await _roleService.GetNormalizedRoleNamesAsync();
            return roleNames.Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<IEnumerable<string>> GetAuthorizedRoleNamesAsync(IEnumerable<string> roleNames)
        {
            var authorizedRoleNames = new List<string>();
            foreach (var roleName in roleNames)
            {
                if (await _authorizationService.AuthorizeAsync(User, CommonPermissions.CreatePermissionForManageUsersInRole(roleName)))
                {
                    authorizedRoleNames.Add(roleName);
                }
            }

            return authorizedRoleNames;
        }
    }
}
