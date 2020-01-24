using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Controllers
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
            IHtmlLocalizer<AdminController> h,
            IStringLocalizer<AdminController> s
            )
        {
            _siteSettingsDisplayManager = siteSettingsDisplayManager;
            _siteService = siteService;
            _notifier = notifier;
            _authorizationService = authorizationService;
            H = h;
            S = s;
        }

        IHtmlLocalizer H { get; set; }
        IStringLocalizer S { get; set; }

        public async Task<IActionResult> Index(string groupId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageGroupSettings, (object)groupId))
            {
                return Unauthorized();
            }

            var site = await _siteService.GetSiteSettingsAsync();

            var viewModel = new AdminIndexViewModel
            {
                GroupId = groupId,
                Shape = await _siteSettingsDisplayManager.BuildEditorAsync(site, this, false, groupId)
            };

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

            var site = await _siteService.LoadSiteSettingsAsync();

            var viewModel = new AdminIndexViewModel
            {
                GroupId = groupId,
                Shape = await _siteSettingsDisplayManager.UpdateEditorAsync(site, this, false, groupId)
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
