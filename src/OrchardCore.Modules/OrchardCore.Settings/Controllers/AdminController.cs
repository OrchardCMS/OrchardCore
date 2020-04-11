using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Controllers
{
    public class AdminController : Controller
    {
        private readonly IDisplayManager<ISite> _siteSettingsDisplayManager;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IHtmlLocalizer H;

        public AdminController(
            ISiteService siteService,
            IDisplayManager<ISite> siteSettingsDisplayManager,
            IAuthorizationService authorizationService,
            INotifier notifier,
            IHtmlLocalizer<AdminController> h,
            IUpdateModelAccessor updateModelAccessor)
        {
            _siteSettingsDisplayManager = siteSettingsDisplayManager;
            _siteService = siteService;
            _notifier = notifier;
            _authorizationService = authorizationService;
            _updateModelAccessor = updateModelAccessor;
            H = h;
        }

        public async Task<IActionResult> Index(string groupId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageGroupSettings, (object)groupId))
            {
                return Forbid();
            }

            var site = await _siteService.GetSiteSettingsAsync();

            var viewModel = new AdminIndexViewModel
            {
                GroupId = groupId,
                Shape = await _siteSettingsDisplayManager.BuildEditorAsync(site, _updateModelAccessor.ModelUpdater, false, groupId)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        public async Task<IActionResult> IndexPost(string groupId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageGroupSettings, (object)groupId))
            {
                return Forbid();
            }

            var site = await _siteService.LoadSiteSettingsAsync();

            var viewModel = new AdminIndexViewModel
            {
                GroupId = groupId,
                Shape = await _siteSettingsDisplayManager.UpdateEditorAsync(site, _updateModelAccessor.ModelUpdater, false, groupId)
            };

            if (ModelState.IsValid)
            {
                await _siteService.UpdateSiteSettingsAsync(site);

                _notifier.Success(H["Site settings updated successfully."]);

                return RedirectToAction(nameof(Index), new { groupId });
            }

            return View(viewModel);
        }
    }
}
