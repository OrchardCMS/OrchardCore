using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
using Orchard.Settings.Services;
using Orchard.Settings.ViewModels;
using Orchard.Hosting;
using Orchard.Environment.Shell;

namespace Orchard.Settings.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly ISiteSettingsDisplayManager _siteSettingsDisplayManager;
        private readonly ISiteService _siteService;
        private readonly IOrchardHost _orchardHost;
        private readonly ShellSettings _shellSettings;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;

        public AdminController(
            ISiteService siteService,
            ISiteSettingsDisplayManager siteSettingsDisplayManager,
            IAuthorizationService authorizationService,
            IOrchardHost orchardHost,
            ShellSettings shellSettings,
            INotifier notifier,
            IHtmlLocalizer<AdminController> h)
        {
            _siteSettingsDisplayManager = siteSettingsDisplayManager;
            _siteService = siteService;
            _orchardHost = orchardHost;
            _shellSettings = shellSettings;
            _notifier = notifier;
            _authorizationService = authorizationService;
            H = h;
        }

        IHtmlLocalizer H { get; set; }

        public async Task<IActionResult> Index(string groupId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSettings))
            {
                return Unauthorized();
            }

            var viewModel = new AdminIndexViewModel();

            viewModel.GroupId = groupId;
            viewModel.Shape = await _siteSettingsDisplayManager.BuildEditorAsync(this, groupId);

            return View(viewModel);
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        public async Task<IActionResult> IndexPost(string groupId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSettings))
            {
                return Unauthorized();
            }

            var viewModel = new AdminIndexViewModel();

            viewModel.GroupId = groupId;
            viewModel.Shape = await _siteSettingsDisplayManager.UpdateEditorAsync(this, groupId);

            if (ModelState.IsValid)
            {
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                await _siteService.UpdateSiteSettingsAsync(siteSettings);

                _notifier.Success(H["Site settings updated successfully."]);

                return RedirectToAction(nameof(Index), new { groupId });
            }

            return View(viewModel);
        }

        public async Task<IActionResult> RestartSite()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.Restart))
            {
                return Unauthorized();
            }

            return View();
        }

        [HttpPost]
        [ActionName(nameof(RestartSite))]
        public async Task<IActionResult> RestartSitePost()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSettings))
            {
                return Unauthorized();
            }
            _orchardHost.ReloadShellContext(_shellSettings);

            return Redirect("~/admin");
        }
    }
}