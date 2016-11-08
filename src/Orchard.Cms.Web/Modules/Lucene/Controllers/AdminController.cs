using System;
using System.Linq;
using System.Threading.Tasks;
using Lucene.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement.Notify;
using Orchard.Utility;

namespace Lucene.Controllers
{
    public class AdminController : Controller
    {
        private readonly LuceneIndexProvider _luceneIndexProvider;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;

        public AdminController(
            LuceneIndexProvider luceneIndexProvider,
            IAuthorizationService authorizationService,
            INotifier notifier,
            IStringLocalizer<AdminController> s,
            IHtmlLocalizer<AdminController> h,
            ILogger<AdminController> logger)
        {
            _luceneIndexProvider = luceneIndexProvider;
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

            viewModel.Indexes = _luceneIndexProvider.List().Select(s => new IndexViewModel { Name = s }).ToArray();

            return View(viewModel);
        }

        public async Task<ActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            var model = new AdminEditViewModel
            {
                IndexName = "",
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

            ValidateModel(model);

            if (_luceneIndexProvider.Exists(model.IndexName))
            {
                ModelState.AddModelError(nameof(AdminEditViewModel.IndexName), S["An index named {0} already exists."]);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                _luceneIndexProvider.CreateIndex(model.IndexName);
            }
            catch (Exception e)
            {
                _notifier.Error(H["An error occurred while creating the index"]);
                Logger.LogError("An error occurred while creating an index", e);
                return View(model);
            }

            _notifier.Success(H["Index <em>{0}</em> created successfully", model.IndexName]);

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

            try
            {
                _luceneIndexProvider.DeleteIndex(model.IndexName);

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
