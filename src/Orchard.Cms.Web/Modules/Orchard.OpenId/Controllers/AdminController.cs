using CryptoHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Notify;
using Orchard.Navigation;
using Orchard.OpenId.Models;
using Orchard.OpenId.Services;
using Orchard.OpenId.ViewModels;
using Orchard.Security.Services;
using Orchard.Settings;
using Orchard.Users.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.OpenId.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer<AdminController> T;
        private readonly IHtmlLocalizer<AdminController> H;
        private readonly ISiteService _siteService;
        private readonly IShapeFactory _shapeFactory;
        private readonly IOpenIdApplicationManager _applicationManager;
        private readonly IRoleProvider _roleProvider;
        private readonly INotifier _notifier;
        private readonly IOpenIdService _openIdService;
        
        public AdminController(
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IStringLocalizer<AdminController> stringLocalizer,
            IAuthorizationService authorizationService,
            IOpenIdApplicationManager applicationManager,
            IRoleProvider roleProvider,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            INotifier notifier,
            IOpenIdService openIdService
            )
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
        }
        
        public async Task<ActionResult> Index(UserIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
                return Unauthorized();

            var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
            if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
                _notifier.Warning(H["Open Id settings are not properly configured."]);
            
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            var results = await _applicationManager.GetAppsAsync(pager.GetStartIndex(),pager.PageSize);

            var pagerShape = _shapeFactory.Create("Pager", new { TotalItemCount = await _applicationManager.GetCount()});

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
                return Unauthorized();
            var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
            if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
                _notifier.Warning(H["Open Id settings are not properly configured."]);
            
            var app = await _applicationManager.FindByIdAsync(id);
            if (app == null)
                return NotFound();

            var roles = await _roleProvider.GetRoleNamesAsync();
            var model = new EditOpenIdApplicationViewModel()
            {
                RoleEntries = roles.Select(r => new RoleEntry() { Name = r }).ToList()
            };

            model.Id = id;
            model.DisplayName = app.DisplayName;
            model.RedirectUri = app.RedirectUri;
            model.LogoutRedirectUri = app.LogoutRedirectUri;
            model.ClientId = app.ClientId;
            model.Type = app.Type;
            model.SkipConsent = app.SkipConsent;
            model.RoleEntries.ForEach(r=> r.Selected = app.RoleNames.Contains(r.Name));

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditOpenIdApplicationViewModel model, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
                return Unauthorized();

            if (model.Type == ClientType.Public)
            {
                if (string.IsNullOrWhiteSpace(model.LogoutRedirectUri))
                {
                    ModelState.AddModelError(nameof(EditOpenIdApplicationViewModel.LogoutRedirectUri), T["Logout Redirect Uri is required for Public apps."]);
                }
                if (string.IsNullOrWhiteSpace(model.RedirectUri))
                {
                    ModelState.AddModelError(nameof(EditOpenIdApplicationViewModel.RedirectUri), T["Redirect Uri is required for Public apps."]);
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var app = await _applicationManager.FindByIdAsync(model.Id);               
                if (app == null)
                    return NotFound();

                app.DisplayName = model.DisplayName;
                app.RedirectUri = model.RedirectUri;
                app.LogoutRedirectUri = model.LogoutRedirectUri;
                app.ClientId = model.ClientId;
                app.Type = model.Type;
                app.SkipConsent = model.SkipConsent;
                app.RoleNames = new List<string>();
                if (app.Type == ClientType.Confidential)
                    app.RoleNames = model.RoleEntries.Where(r => r.Selected).Select(r => r.Name).ToList();

                await _applicationManager.CreateAsync(app);                
                if (returnUrl == null)
                    return RedirectToAction("Index");
                else
                    return LocalRedirect(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Create(string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
                return Unauthorized();

            var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
            if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
                _notifier.Warning(H["Open Id settings are not properly configured."]);

            var roles = await _roleProvider.GetRoleNamesAsync();

            var model = new CreateOpenIdApplicationViewModel()
            {
                RoleEntries = roles.Select(r => new RoleEntry() { Name = r }).ToList()
            };


            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOpenIdApplicationViewModel model, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
                return Unauthorized();
            
            if (model.Type == ClientType.Public)
            {
                if (string.IsNullOrWhiteSpace(model.LogoutRedirectUri))
                {
                    ModelState.AddModelError(nameof(CreateOpenIdApplicationViewModel.LogoutRedirectUri), T["Logout Redirect Uri is required for Public apps."]);
                }
                if (string.IsNullOrWhiteSpace(model.RedirectUri))
                {
                    ModelState.AddModelError(nameof(CreateOpenIdApplicationViewModel.RedirectUri), T["Redirect Uri is required for Public apps."]);
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var roleNames = new List<string>();
                if (model.Type == ClientType.Confidential)
                    roleNames = model.RoleEntries.Where(r => r.Selected).Select(r => r.Name).ToList();

                var openIdApp = new OpenIdApplication { DisplayName = model.DisplayName, RedirectUri = model.RedirectUri, LogoutRedirectUri = model.LogoutRedirectUri, ClientId = model.ClientId, ClientSecret = Crypto.HashPassword(model.ClientSecret), Type = model.Type, SkipConsent = model.SkipConsent, RoleNames = roleNames };

                await _applicationManager.CreateAsync(openIdApp);
                if (returnUrl == null)
                    return RedirectToAction("Index");
                else
                    return LocalRedirect(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            return View("Edit",model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
                return Unauthorized();

            var application = await _applicationManager.FindByIdAsync(id);

            if (application == null)
            {
                return NotFound();
            }

            await _applicationManager.DeleteAsync(application);
            
            return RedirectToAction(nameof(Index));
        }
    }
}
