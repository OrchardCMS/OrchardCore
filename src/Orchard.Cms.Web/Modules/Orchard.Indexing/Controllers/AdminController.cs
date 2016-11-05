using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement.Notify;
using Orchard.Indexing.ViewModels;
using Orchard.Utility;

namespace Orchard.Indexing.Controllers
{
    public class AdminController : Controller
    {
        private readonly IndexManager _indexManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;

        public AdminController(
            IndexManager indexManager,
            IAuthorizationService authorizationService,
            INotifier notifier,
            IStringLocalizer<AdminController> s,
            IHtmlLocalizer<AdminController> h,
            ILogger<AdminController> logger)
        {
            _indexManager = indexManager;
            _authorizationService = authorizationService;
            _notifier = notifier;
            S = s;
            H = h;
            Logger = logger;
        }

        public ILogger Logger { get; }
        public IStringLocalizer S { get; }
        public IHtmlLocalizer H { get; }

        public ActionResult Index()
        {
            var viewModel = new AdminIndexViewModel();

            viewModel.Providers = _indexManager.Providers.Select(x => x.Key).ToList();
            viewModel.Indexes = _indexManager.Providers
                .SelectMany(x => x.Value.List().Select(s => new IndexViewModel { Name = s, Provider = x.Key })
                ).ToArray();

            return View(viewModel);
        }

        public async Task<ActionResult> Create(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            IIndexProvider provider;
            if (!_indexManager.Providers.TryGetValue(id, out provider))
            {
                return NotFound();
            }

            var model = new AdminEditViewModel
            {
                IndexName = "",
                ProviderName = id,
            };

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public async Task<ActionResult> CreatePOST(AdminEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            IIndexProvider provider;
            if (!_indexManager.Providers.TryGetValue(model.ProviderName, out provider))
            {
                return NotFound();
            }

            ValidateModel(model);

            if (provider.Exists(model.IndexName))
            {
                ModelState.AddModelError(nameof(AdminEditViewModel.IndexName), S["An index named {0} already exists."]);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                provider.CreateIndex(model.IndexName);
            }
            catch (Exception e)
            {
                _notifier.Error(H["An error occurred while creating the index"]);
                Logger.LogError("An error occurred while creating the index " + model.ProviderName, e);
                return View(model);
            }

            _notifier.Success(H["Index <em>{0}</em> created successfully", model.ProviderName]);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Update(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            // _indexingService.UpdateIndex(id);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Rebuild(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            // _indexingService.RebuildIndex(id);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(AdminEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            IIndexProvider provider;
            if (!_indexManager.Providers.TryGetValue(model.ProviderName, out provider))
            {
                return NotFound();
            }

            try
            {
                provider.DeleteIndex(model.IndexName);

                _notifier.Success(H["Index <em>{0}</em> deleted successfully", model.IndexName]);
            }
            catch(Exception e)
            {
                _notifier.Error(H["An error occurred while deleting the index"]);
                Logger.LogError("An error occurred while deleting the index " + model.IndexName, e);
            }

            return RedirectToAction("Index");
        }

        private void ValidateModel(AdminEditViewModel model)
        {
            if (String.IsNullOrWhiteSpace(model.IndexName))
            {
                ModelState.AddModelError(nameof(AdminEditViewModel.IndexName), S["The index name is required."]);
            }
            else if (model.IndexName.ToSafeName() != model.IndexName)
            {
                ModelState.AddModelError(nameof(AdminEditViewModel.IndexName), S["The index name contains unallowed chars."]);
            }
        }
    }
}
