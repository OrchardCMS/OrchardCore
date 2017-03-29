using System;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers.Classic;
using Orchard.Lucene.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement.Notify;
using Microsoft.AspNetCore.Mvc.Modules.Utilities;

namespace Orchard.Lucene.Controllers
{
    public class AdminController : Controller
    {
        private readonly LuceneIndexProvider _luceneIndexProvider;
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;

        public AdminController(
            LuceneIndexProvider luceneIndexProvider,
            LuceneIndexingService luceneIndexingService,
            IAuthorizationService authorizationService,
            INotifier notifier,
            IStringLocalizer<AdminController> s,
            IHtmlLocalizer<AdminController> h,
            ILogger<AdminController> logger)
        {
            _luceneIndexProvider = luceneIndexProvider;
            _luceneIndexingService = luceneIndexingService;
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
                // We call Rebuild in order to reset the index state cursor too in case the same index
                // name was also used previously.
                _luceneIndexingService.RebuildIndex(model.IndexName);
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
        public async Task<ActionResult> Reset(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            if (!_luceneIndexProvider.Exists(id))
            {
                return NotFound();
            }

            _luceneIndexingService.ResetIndex(id);

            _notifier.Success(H["Index <em>{0}</em> resetted successfully", id]);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Rebuild(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            if (!_luceneIndexProvider.Exists(id))
            {
                return NotFound();
            }

            _luceneIndexingService.RebuildIndex(id);

            _notifier.Success(H["Index <em>{0}</em> rebuilt successfully", id]);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(AdminEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            if (!_luceneIndexProvider.Exists(model.IndexName))
            {
                return NotFound();
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

        public async Task<IActionResult> Query(AdminQueryViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            if (!_luceneIndexProvider.Exists(model.IndexName))
            {
                return NotFound();
            }

            if (String.IsNullOrWhiteSpace(model.Query))
            {
                return View(model);
            }

            var luceneSettings = await _luceneIndexingService.GetLuceneSettingsAsync();

            var queryParser = new QueryParser(LuceneSettings.DefaultVersion, "", new StandardAnalyzer(LuceneSettings.DefaultVersion));
            var query = queryParser.Parse(model.Query);

            _luceneIndexProvider.Search(model.IndexName, searcher =>
            {
                var docs = searcher.Search(query, 10);
                model.Documents = docs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)).ToList();
            });

            return View(model);
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
