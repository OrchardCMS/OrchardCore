using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
using Orchard.Settings.ViewModels;

namespace Orchard.Settings.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IDisplayManager<ISite> _siteSettingsDisplayManager;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;

        public AdminController(
            ISiteService siteService,
            IDisplayManager<ISite> siteSettingsDisplayManager,
            IAuthorizationService authorizationService,
            INotifier notifier,
            IHtmlLocalizer<AdminController> h)
        {
            _siteSettingsDisplayManager = siteSettingsDisplayManager;
            _siteService = siteService;
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
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSettings))
            {
                return Unauthorized();
            }

            var site = await _siteService.GetSiteSettingsAsync();

            var viewModel = new AdminIndexViewModel();

            viewModel.GroupId = groupId;
            viewModel.Shape = await _siteSettingsDisplayManager.UpdateEditorAsync(site, this, groupId);

            if (ModelState.IsValid)
            {
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                await _siteService.UpdateSiteSettingsAsync(siteSettings);

                _notifier.Success(H["Site settings updated successfully."]);

                return RedirectToAction(nameof(Index), new { groupId });
            }

            return View(viewModel);
        }
    }
}