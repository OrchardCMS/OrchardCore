using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
using Orchard.Settings.Services;
using Orchard.Settings.ViewModels;

namespace Orchard.Settings.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly ISiteSettingsDisplayManager _siteSettingsDisplayManager;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;

        public AdminController(
            ISiteService siteService,
            ISiteSettingsDisplayManager siteSettingsDisplayManager,
            INotifier notifier,
            IHtmlLocalizer<AdminController> h)
        {
            _siteSettingsDisplayManager = siteSettingsDisplayManager;
            _siteService = siteService;
            _notifier = notifier;
            H = h;
        }

        IHtmlLocalizer H { get; set; }

        public async Task<IActionResult> Index(string groupId)
        {
            var viewModel = new AdminIndexViewModel();

            viewModel.GroupId = groupId;
            viewModel.Shape = await _siteSettingsDisplayManager.BuildEditorAsync(this, groupId);
            
            return View(viewModel);
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        public async Task<IActionResult> IndexPost(string groupId)
        {
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
    }
}
