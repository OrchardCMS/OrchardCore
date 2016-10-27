using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Models;
using Orchard.Hosting;
using Orchard.Tenants.ViewModels;

namespace Orchard.Tenants.Controllers
{
    public class AdminController : Controller
    {
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IAuthorizationService _authorizationService;

        public AdminController(
            IOrchardHost orchardHost, 
            IAuthorizationService authorizationService,
            IShellSettingsManager shellSettingsManager,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer)
        {
            _orchardHost = orchardHost;
            _authorizationService = authorizationService;
            _shellSettingsManager = shellSettingsManager;
            S = stringLocalizer;
            H = htmlLocalizer;
        }

        public IStringLocalizer S { get; set; }
        public IHtmlLocalizer H { get; set; }

        public IActionResult Index()
        {
            var shells = _orchardHost.ListShellContexts();

            var model = new AdminIndexViewModel
            {
                ShellSettingsEntries = shells.Select(x => new ShellSettingsEntry { Name = x.Settings.Name, ShellSettings = x.Settings }).ToList()
            };

            return View(model);
        }


        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            var model = new EditTenantViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EditTenantViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                ValidateViewModel(model);
            }

            if (ModelState.IsValid)
            {
                var shellSettings = new ShellSettings
                {
                    Name = "Test1",
                    RequestUrlPrefix = "test1",
                    State = TenantState.Uninitialized
                };

                _shellSettingsManager.SaveSettings(shellSettings);
                var shellContext = _orchardHost.GetOrCreateShellContext(shellSettings);

                return Redirect("~/test1");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private void ValidateViewModel(EditTenantViewModel model)
        {
            if (String.IsNullOrWhiteSpace(model.SiteName))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.SiteName), S["The site name is mandatory."]);
            }
        }
    }
}
