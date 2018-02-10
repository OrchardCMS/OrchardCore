using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenIddict.Core;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Models;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Services.Managers;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Security.Services;
using OrchardCore.Settings;
using OrchardCore.Users;

namespace OrchardCore.OpenId.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer<AdminController> T;
        private readonly IHtmlLocalizer<AdminController> H;
        private readonly ISiteService _siteService;
        private readonly IShapeFactory _shapeFactory;
        private readonly IRoleProvider _roleProvider;
        private readonly OpenIdApplicationManager _applicationManager;
        private readonly INotifier _notifier;
        private readonly IOpenIdService _openIdService;
        private readonly IEnumerable<IPasswordValidator<IUser>> _passwordValidators;
        private readonly UserManager<IUser> _userManager;
        private readonly IOptions<IdentityOptions> _identityOptions;

        public AdminController(
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IStringLocalizer<AdminController> stringLocalizer,
            IAuthorizationService authorizationService,
            IRoleProvider roleProvider,
            OpenIdApplicationManager applicationManager,
            IEnumerable<IPasswordValidator<IUser>> passwordValidators,
            UserManager<IUser> userManager,
            IOptions<IdentityOptions> identityOptions,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            INotifier notifier,
            IOpenIdService openIdService)
        {
            _shapeFactory = shapeFactory;
            _siteService = siteService;
            T = stringLocalizer;
            H = htmlLocalizer;
            _authorizationService = authorizationService;
            _applicationManager = applicationManager;
            _roleProvider = roleProvider;
            _notifier = notifier;
            _openIdService = openIdService;
            _passwordValidators = passwordValidators;
            _userManager = userManager;
            _identityOptions = identityOptions;
        }

        public async Task<ActionResult> Index(PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
            {
                return Unauthorized();
            }

            var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
            if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
            {
                _notifier.Warning(H["OpenID Connect settings are not properly configured."]);
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            var model = new OpenIdApplicationsIndexViewModel
            {
                Pager = await _shapeFactory.CreateAsync("Pager", new
                {
                    TotalItemCount = await _applicationManager.CountAsync()
                })
            };

            foreach (var application in await _applicationManager.ListAsync(pager.PageSize, pager.GetStartIndex()))
            {
                model.Applications.Add(new OpenIdApplicationEntry
                {
                    DisplayName = await _applicationManager.GetDisplayNameAsync(application),
                    Id = await _applicationManager.GetPhysicalIdAsync(application)
                });
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(string id, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
            {
                return Unauthorized();
            }

            var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
            if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
            {
                _notifier.Warning(H["OpenID Connect settings are not properly configured."]);
            }

            var application = await _applicationManager.FindByPhysicalIdAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            var permissions = await _applicationManager.GetPermissionsAsync(application);

            var model = new EditOpenIdApplicationViewModel
            {
                AllowAuthorizationCodeFlow = permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode),
                AllowClientCredentialsFlow = permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials),
                AllowImplicitFlow = permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.Implicit),
                AllowPasswordFlow = permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.Password),
                AllowRefreshTokenFlow = permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.RefreshToken),
                ClientId = await _applicationManager.GetClientIdAsync(application),
                DisplayName = await _applicationManager.GetDisplayNameAsync(application),
                Id = await _applicationManager.GetPhysicalIdAsync(application),
                LogoutRedirectUri = (await _applicationManager.GetPostLogoutRedirectUrisAsync(application)).FirstOrDefault(),
                RedirectUri = (await _applicationManager.GetRedirectUrisAsync(application)).FirstOrDefault(),
                SkipConsent = !await _applicationManager.IsConsentRequiredAsync(application),
                Type = (ClientType) Enum.Parse(typeof(ClientType), await _applicationManager.GetClientTypeAsync(application), ignoreCase: true)
            };

            foreach (var role in await _roleProvider.GetRoleNamesAsync())
            {
                model.RoleEntries.Add(new RoleEntry
                {
                    Name = role,
                    Selected = await _applicationManager.IsInRoleAsync(application, role)
                });
            }

            ViewData["OpenIdSettings"] = openIdSettings;
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditOpenIdApplicationViewModel model, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
            {
                return Unauthorized();
            }

            if (model.Type == ClientType.Public && !string.IsNullOrEmpty(model.ClientSecret))
            {
                ModelState.AddModelError(nameof(model.ClientSecret), T["No client secret can be set for public applications."]);
            }
            else if (model.UpdateClientSecret)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                await ValidateClientSecretAsync(user, model.ClientSecret, (key, message) => ModelState.AddModelError(key, message));
            }

            IOpenIdApplication application = null;

            if (ModelState.IsValid)
            {
                application = await _applicationManager.FindByPhysicalIdAsync(model.Id);
                if (application == null)
                {
                    return NotFound();
                }

                if (model.Type == ClientType.Confidential && !model.UpdateClientSecret &&
                    await _applicationManager.IsPublicAsync(application))
                {
                    ModelState.AddModelError(nameof(model.UpdateClientSecret), T["Setting a new client secret is required"]);
                }

                var other = await _applicationManager.FindByClientIdAsync(model.ClientId);
                if (other != null && !string.Equals(
                    await _applicationManager.GetIdAsync(other),
                    await _applicationManager.GetIdAsync(application), StringComparison.Ordinal))
                {
                    ModelState.AddModelError(nameof(model.ClientId), T["The client identifier is already taken by another application."]);
                }
            }

            if (!ModelState.IsValid)
            {
                var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
                if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
                {
                    _notifier.Warning(H["OpenID Connect settings are not properly configured."]);
                }

                ViewData["OpenIdSettings"] = openIdSettings;
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            await _applicationManager.UpdateAsync(application, model);

            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index");
            }

            return LocalRedirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
            {
                return Unauthorized();
            }

            var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
            if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
            {
                _notifier.Warning(H["OpenID Connect settings are not properly configured."]);
            }

            var model = new CreateOpenIdApplicationViewModel();

            foreach (var role in await _roleProvider.GetRoleNamesAsync())
            {
                model.RoleEntries.Add(new RoleEntry { Name = role });
            }

            ViewData["OpenIdSettings"] = openIdSettings;
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOpenIdApplicationViewModel model, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
            {
                return Unauthorized();
            }

            if (model.Type == ClientType.Confidential)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                await ValidateClientSecretAsync(user, model.ClientSecret, (key, message) => ModelState.AddModelError(key, message));
            }
            else if (model.Type == ClientType.Public && !string.IsNullOrEmpty(model.ClientSecret))
            {
                ModelState.AddModelError(nameof(model.ClientSecret), T["No client secret can be set for public applications."]);
            }

            if (await _applicationManager.FindByClientIdAsync(model.ClientId) != null)
            {
                ModelState.AddModelError(nameof(model.ClientId), T["The client identifier is already taken by another application."]);
            }

            if (!ModelState.IsValid)
            {
                var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
                if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
                {
                    _notifier.Warning(H["OpenID Connect settings are not properly configured."]);
                }

                ViewData["OpenIdSettings"] = openIdSettings;
                ViewData["ReturnUrl"] = returnUrl;
                return View("Create", model);
            }

            await _applicationManager.CreateAsync(model);

            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index");
            }

            return LocalRedirect(returnUrl);
        }

        private async Task<bool> ValidateClientSecretAsync(IUser user, string password, Action<string, string> reportError)
        {
            if (string.IsNullOrEmpty(password))
            {
                reportError("ClientSecret", T["The client secret is required for confidential applications."]);
                return false;
            }

            var result = true;
            foreach (var v in _passwordValidators)
            {
                var validationResult = await v.ValidateAsync(_userManager, user, password);
                if (!validationResult.Succeeded)
                {
                    result = false;
                    foreach (var error in validationResult.Errors)
                    {
                        switch (error.Code)
                        {
                        case "PasswordRequiresDigit":
                            reportError("ClientSecret", T["Passwords must have at least one digit ('0'-'9')."]);
                            break;
                        case "PasswordRequiresLower":
                            reportError("ClientSecret", T["Passwords must have at least one lowercase ('a'-'z')."]);
                            break;
                        case "PasswordRequiresUpper":
                            reportError("ClientSecret", T["Passwords must have at least one uppercase('A'-'Z')."]);
                            break;
                        case "PasswordRequiresNonAlphanumeric":
                            reportError("ClientSecret", T["Passwords must have at least one non letter or digit character."]);
                            break;
                        case "PasswordTooShort":
                            reportError("ClientSecret", T["Passwords must be at least {0} characters.", _identityOptions.Value.Password.RequiredLength]);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
            {
                return Unauthorized();
            }

            var application = await _applicationManager.FindByPhysicalIdAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            await _applicationManager.DeleteAsync(application);

            return RedirectToAction(nameof(Index));
        }
    }
}
