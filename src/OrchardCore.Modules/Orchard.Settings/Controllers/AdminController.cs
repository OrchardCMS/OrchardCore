using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Newtonsoft.Json;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
using Orchard.Settings.ViewModels;
using Orchard.Hosting;
using Orchard.Environment.Shell;

namespace Orchard.Settings.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IDisplayManager<ISite> _siteSettingsDisplayManager;
        private readonly ISiteService _siteService;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;

        public AdminController(
            ISiteService siteService,
            IDisplayManager<ISite> siteSettingsDisplayManager,
            IAuthorizationService authorizationService,
            IShellHost shellHost,
            ShellSettings shellSettings,
            INotifier notifier,
            IHtmlLocalizer<AdminController> h)
        {
            _siteSettingsDisplayManager = siteSettingsDisplayManager;
            _siteService = siteService;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _notifier = notifier;
            _authorizationService = authorizationService;
            H = h;
        }

        IHtmlLocalizer H { get; set; }

        public async Task<IActionResult> Index(string groupId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageGroupSettings, (object) groupId))
            {
                return Unauthorized();
            }

            var site = await _siteService.GetSiteSettingsAsync();

            var viewModel = new AdminIndexViewModel();

            viewModel.GroupId = groupId;
            viewModel.Shape = await _siteSettingsDisplayManager.BuildEditorAsync(site, this, groupId);

            return View(viewModel);
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        public async Task<IActionResult> IndexPost(string groupId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageGroupSettings, (object)groupId))
            {
                return Unauthorized();
            }

            var cachedSite = await _siteService.GetSiteSettingsAsync();

            // Clone the settings as the driver will update it and as it's a globally cached object
            // it would stay this way even on validation errors.

            var site = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(cachedSite, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }), cachedSite.GetType()) as ISite;

            var viewModel = new AdminIndexViewModel();

            viewModel.GroupId = groupId;
            viewModel.Shape = await _siteSettingsDisplayManager.UpdateEditorAsync(site, this, groupId);

            if (ModelState.IsValid)
            {
                await _siteService.UpdateSiteSettingsAsync(site);

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
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.Restart))
            {
                return Unauthorized();
            }
            _shellHost.ReloadShellContext(_shellSettings);

            return Redirect("RestartSite");
        }
    }
}