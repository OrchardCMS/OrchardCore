using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OpenIddict.Core;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Notify;
using Orchard.Navigation;
using Orchard.OpenId.Models;
using Orchard.OpenId.Services;
using Orchard.OpenId.ViewModels;
using Orchard.Settings;

namespace Orchard.OpenId.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer<AdminController> T;
        private readonly IHtmlLocalizer<AdminController> H;
        private readonly ISiteService _siteService;
        private readonly IShapeFactory _shapeFactory;
        private readonly OpenIddictApplicationManager<OpenIdApplication> _applicationManager;
        private readonly OpenIdApplicationStore _applicationStore;
        private readonly INotifier _notifier;
        private readonly IOpenIdService _openIdService;

        public AdminController(
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IStringLocalizer<AdminController> stringLocalizer,
            IAuthorizationService authorizationService,
            OpenIddictApplicationManager<OpenIdApplication> applicationManager,
            OpenIdApplicationStore applicationStore,
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
            _applicationStore = applicationStore;
            _notifier = notifier;
            _openIdService = openIdService;
        }

        public async Task<ActionResult> Index(PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
                return Unauthorized();

            var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
            if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
                _notifier.Warning(H["OpenID Connect settings are not properly configured."]);

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
                return Unauthorized();

            var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
            if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
                _notifier.Warning(H["OpenID Connect settings are not properly configured."]);

            var application = await _applicationManager.FindByIdAsync(id, HttpContext.RequestAborted);
            if (application == null)
                return NotFound();

            var model = new EditOpenIdApplicationViewModel()
            {
                Id = id,
                DisplayName = application.DisplayName,
                RedirectUri = application.RedirectUri,
                LogoutRedirectUri = application.LogoutRedirectUri,
                ClientId = application.ClientId,
                Type = application.Type,
                SkipConsent = application.SkipConsent,
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
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
                if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
                    _notifier.Warning(H["OpenID Connect settings are not properly configured."]);

                ViewData["OpenIdSettings"] = openIdSettings;
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var application = await _applicationManager.FindByIdAsync(model.Id, HttpContext.RequestAborted);
            if (application == null)
                return NotFound();

            await TryUpdateModelAsync(application);
            await _applicationManager.UpdateAsync(application, HttpContext.RequestAborted);

            if (returnUrl == null)
                return RedirectToAction("Index");

            return LocalRedirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
                return Unauthorized();

            var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
            if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
                _notifier.Warning(H["OpenID Connect settings are not properly configured."]);

            ViewData["OpenIdSettings"] = openIdSettings;
            ViewData["ReturnUrl"] = returnUrl;
            return View(new CreateOpenIdApplicationViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOpenIdApplicationViewModel model, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
                if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
                    _notifier.Warning(H["OpenID Connect settings are not properly configured."]);

                ViewData["OpenIdSettings"] = openIdSettings;
                ViewData["ReturnUrl"] = returnUrl;
                return View("Create", model);
            }

            var application = new OpenIdApplication
            {
                DisplayName = model.DisplayName,
                RedirectUri = model.RedirectUri,
                LogoutRedirectUri = model.LogoutRedirectUri,
                ClientId = model.ClientId,
                Type = model.Type,
                SkipConsent = model.SkipConsent,
                AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow,
                AllowClientCredentialsFlow = model.AllowClientCredentialsFlow,
                AllowImplicitFlow = model.AllowImplicitFlow,
                AllowPasswordFlow = model.AllowPasswordFlow,
                AllowRefreshTokenFlow = model.AllowRefreshTokenFlow,
                AllowHybridFlow = model.AllowHybridFlow
            };

            if (model.Type == ClientType.Confidential)
            {
                await _applicationManager.CreateAsync(application, model.ClientSecret, HttpContext.RequestAborted);
            }

            else
            {
                await _applicationManager.CreateAsync(application, HttpContext.RequestAborted);
            }

            if (returnUrl == null)
                return RedirectToAction("Index");

            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
                return Unauthorized();

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
