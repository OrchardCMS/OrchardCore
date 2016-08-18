using CryptoHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement;
using Orchard.Navigation;
using Orchard.Settings;
using Orchard.OpenId.Indexes;
using Orchard.OpenId.Models;
using Orchard.Users.ViewModels;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YesSql.Core.Services;
using Orchard.OpenId.ViewModels;
using Orchard.OpenId.Services;
using Orchard.Security;
using Orchard.Roles.Services;
using System.Collections.Generic;

namespace Orchard.OpenId.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer T;
        private readonly ISiteService _siteService;
        private readonly IShapeFactory _shapeFactory;
        private readonly IOpenIdApplicationManager _applicationManager;
        private readonly IRoleManager _roleManager;

        public AdminController(
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IStringLocalizer<AdminController> stringLocalizer,
            IAuthorizationService authorizationService,
            IOpenIdApplicationManager applicationManager,
            IRoleManager roleManager
            )
        {
            _shapeFactory = shapeFactory;
            _siteService = siteService;
            T = stringLocalizer;
            _authorizationService = authorizationService;
            _applicationManager = applicationManager;
            _roleManager = roleManager;
        }

        public async Task<ActionResult> Index(UserIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
            {
                return Unauthorized();
            }

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

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
                return Unauthorized();

            var app = await _applicationManager.FindByIdAsync(id);
            if (app == null)
                return NotFound();

            var roles = await _roleManager.GetRolesAsync();

            var model = new EditOpenIdApplicationViewModel() {
                Id = id,
                DisplayName = app.DisplayName,
                RedirectUri = app.RedirectUri,
                LogoutRedirectUri = app.LogoutRedirectUri,
                ClientId = app.ClientId,
                Type = app.Type,
                SkipConsent = app.SkipConsent,
                RoleEntries = roles.Roles.Select(r => new RoleEntry() { Name = r.RoleName, Selected = app.RoleNames.Contains(r.RoleName) }).ToList()
            };
            
            return View(model);
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditOpenIdApplicationViewModel model, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
                return Unauthorized();
            
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

            var roles = await _roleManager.GetRolesAsync();

            var model = new CreateOpenIdApplicationViewModel()
            {
                RoleEntries = roles.Roles.Select(r => new RoleEntry() { Name = r.RoleName }).ToList()
            };


            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOpenIdApplicationViewModel model, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOpenIdApplications))
                return Unauthorized();

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var roleNames = new List<string>();
                if (model.Type == ClientType.Confidential)
                    roleNames = model.RoleEntries.Where(r => r.Selected).Select(r => r.Name).ToList();

                var openIdApp = new OpenIdApplication { DisplayName = model.DisplayName, RedirectUri = model.RedirectUri, LogoutRedirectUri = model.LogoutRedirectUri, ClientId = model.ClientId, ClientSecret = Crypto.HashPassword(model.Password), Type = model.Type, SkipConsent = model.SkipConsent, RoleNames = roleNames };
                
                await _applicationManager.CreateAsync(openIdApp);
                if (returnUrl == null)
                    return RedirectToAction("Index");
                else
                    return LocalRedirect(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
    }
}
