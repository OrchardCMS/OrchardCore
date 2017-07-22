using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenIddict.Core;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Notify;
using Orchard.Navigation;
using Orchard.OpenId.Models;
using Orchard.OpenId.Services;
using Orchard.OpenId.ViewModels;
using Orchard.Security.Services;
using Orchard.Settings;
using Orchard.Users.Models;

namespace Orchard.OpenId.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer<AdminController> T;
        private readonly IHtmlLocalizer<AdminController> H;
        private readonly ISiteService _siteService;
        private readonly IShapeFactory _shapeFactory;
        private readonly IRoleProvider _roleProvider;
        private readonly OpenIddictApplicationManager<OpenIdApplication> _applicationManager;
        private readonly OpenIdApplicationStore _applicationStore;
        private readonly INotifier _notifier;
        private readonly IOpenIdService _openIdService;
        private readonly IEnumerable<IPasswordValidator<User>> _passwordValidators;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<IdentityOptions> _identityOptions;

        public AdminController(
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IStringLocalizer<AdminController> stringLocalizer,
            IAuthorizationService authorizationService,
            IRoleProvider roleProvider,
            OpenIddictApplicationManager<OpenIdApplication> applicationManager,
            OpenIdApplicationStore applicationStore,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            UserManager<User> userManager,
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
            _applicationStore = applicationStore;
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

            var results = await _applicationStore.GetAppsAsync(pager.GetStartIndex(), pager.PageSize);

            var pagerShape = _shapeFactory.Create("Pager", new { TotalItemCount = await _applicationStore.GetCount() });

            var model = new OpenIdApplicationsIndexViewModel
            {
                Applications = results
                    .Select(x => new OpenIdApplicationEntry { Application = x })
                    .ToList(),
                Pager = pagerShape
            };

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

            var application = await _applicationManager.FindByIdAsync(id, HttpContext.RequestAborted);
            if (application == null)
            {
                return NotFound();
            }

            var model = new EditOpenIdApplicationViewModel()
            {
                Id = id,
                DisplayName = application.DisplayName,
                RedirectUri = application.RedirectUri,
                LogoutRedirectUri = application.LogoutRedirectUri,
                ClientId = application.ClientId,
                Type = application.Type,
                SkipConsent = application.SkipConsent,
                RoleEntries = (await _roleProvider.GetRoleNamesAsync())
                    .Select(r => new RoleEntry()
                    {
                        Name = r,
                        Selected = application.RoleNames.Contains(r),
                    }).ToList(),
                AllowAuthorizationCodeFlow = application.AllowAuthorizationCodeFlow,
                AllowClientCredentialsFlow = application.AllowClientCredentialsFlow,
                AllowImplicitFlow = application.AllowImplicitFlow,
                AllowPasswordFlow = application.AllowPasswordFlow,
                AllowRefreshTokenFlow = application.AllowRefreshTokenFlow
            };

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
            
            OpenIdApplication application = null;

            if (ModelState.IsValid)
            {
                application = await _applicationManager.FindByIdAsync(model.Id, HttpContext.RequestAborted);
                if (application == null)
                {
                    return NotFound();
                }

                if (application.Type == ClientType.Public && model.Type == ClientType.Confidential && !model.UpdateClientSecret)
                {
                    ModelState.AddModelError(nameof(model.UpdateClientSecret), T["Setting a new client secret is required"]);
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

            // If the application was confidential and is now public, the client secret must be reset.
            if (application.Type == ClientType.Confidential && model.Type == ClientType.Public)
            {
                model.UpdateClientSecret = true;
                model.ClientSecret = null;
            }

            application.DisplayName = model.DisplayName;
            application.RedirectUri = model.RedirectUri;
            application.LogoutRedirectUri = model.LogoutRedirectUri;
            application.ClientId = model.ClientId;
            application.Type = model.Type;
            application.SkipConsent = model.SkipConsent;
            application.AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow;
            application.AllowClientCredentialsFlow = model.AllowClientCredentialsFlow;
            application.AllowImplicitFlow = model.AllowImplicitFlow;
            application.AllowPasswordFlow = model.AllowPasswordFlow;
            application.AllowRefreshTokenFlow = model.AllowRefreshTokenFlow;
            application.AllowHybridFlow = model.AllowHybridFlow;

            application.RoleNames = new List<string>();
            if (application.Type == ClientType.Confidential && application.AllowClientCredentialsFlow)
            {
                application.RoleNames = model.RoleEntries.Where(r => r.Selected).Select(r => r.Name).ToList();
            }

            if (model.UpdateClientSecret)
            {
                await _applicationManager.UpdateAsync(application, model.ClientSecret, HttpContext.RequestAborted);
            }
            else
            {
                await _applicationManager.UpdateAsync(application, HttpContext.RequestAborted);
            }

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

            var model = new CreateOpenIdApplicationViewModel()
            {
                RoleEntries = (await _roleProvider.GetRoleNamesAsync()).Select(r => new RoleEntry() { Name = r }).ToList()
            };

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

            var roleNames = new List<string>();
            if (model.Type == ClientType.Confidential && model.AllowClientCredentialsFlow)
            {
                roleNames = model.RoleEntries.Where(r => r.Selected).Select(r => r.Name).ToList();
            }
            
            var application = new OpenIdApplication
            {
                DisplayName = model.DisplayName,
                RedirectUri = model.RedirectUri,
                LogoutRedirectUri = model.LogoutRedirectUri,
                ClientId = model.ClientId,
                Type = model.Type,
                SkipConsent = model.SkipConsent,
                RoleNames = roleNames,
                AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow,
                AllowClientCredentialsFlow = model.AllowClientCredentialsFlow,
                AllowImplicitFlow = model.AllowImplicitFlow,
                AllowPasswordFlow = model.AllowPasswordFlow,
                AllowRefreshTokenFlow = model.AllowRefreshTokenFlow,
                AllowHybridFlow = model.AllowHybridFlow
            };

            await _applicationManager.CreateAsync(application, model.ClientSecret, HttpContext.RequestAborted);

            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index");
            }
            return LocalRedirect(returnUrl);
        }
        private async Task<bool> ValidateClientSecretAsync(User user, string password, Action<string, string> reportError)
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

            var application = await _applicationManager.FindByIdAsync(id, HttpContext.RequestAborted);
            if (application == null)
            {
                return NotFound();
            }

            await _applicationManager.DeleteAsync(application, HttpContext.RequestAborted);

            return RedirectToAction(nameof(Index));
        }
    }
}
