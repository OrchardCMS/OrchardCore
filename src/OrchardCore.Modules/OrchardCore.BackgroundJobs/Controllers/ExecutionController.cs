using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.BackgroundJobs.Indexes;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.BackgroundJobs.Services;
using OrchardCore.BackgroundJobs.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Settings;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Services;

namespace OrchardCore.BackgroundJobs.Controllers
{
    [Admin]
    public class ExecutionController : Controller
    {
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IBackgroundJobStore _backgroundJobStore;
        private readonly IDisplayManager<BackgroundJobExecution> _backgroundJobExecutionDisplayManager;
        private readonly IDisplayManager<BackgroundJobIndexOptions> _backgroundJobOptionsDisplayManager;
        private readonly IBackgroundJobsAdminListQueryService _backgroundJobsAdminListQueryService;
        private readonly ISession _session;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IClock _clock;
        private readonly IShapeFactory _shapeFactory;
        private readonly ILogger _logger;

        private readonly dynamic New;
        private readonly IHtmlLocalizer H;
        private readonly IStringLocalizer S;

        public ExecutionController(
            IBackgroundJobService backgroundJobService,
            IBackgroundJobStore backgroundJobStore,
            IDisplayManager<BackgroundJobExecution> backgroundJobExecutionDisplayManager,
            IDisplayManager<BackgroundJobIndexOptions> backgroundJobOptionsDisplayManager,
            IBackgroundJobsAdminListQueryService backgroundJobsAdminListQueryService,
            ISession session,
            IAuthorizationService authorizationService,
            ISiteService siteService,
            INotifier notifier,
            IUpdateModelAccessor updateModelAccessor,
            IClock clock,
            IShapeFactory shapeFactory,
            ILogger<ExecutionController> logger,
            IHtmlLocalizer<ExecutionController> htmlLocalizer,
            IStringLocalizer<ExecutionController> stringLocalizer)
        {
            _backgroundJobService = backgroundJobService;
            _backgroundJobExecutionDisplayManager = backgroundJobExecutionDisplayManager;
            _backgroundJobOptionsDisplayManager = backgroundJobOptionsDisplayManager;
            _authorizationService = authorizationService;
            _session = session;
            _backgroundJobStore = backgroundJobStore;
            _notifier = notifier;
            _clock = clock;
            _siteService = siteService;
            _backgroundJobsAdminListQueryService = backgroundJobsAdminListQueryService;
            _updateModelAccessor = updateModelAccessor;
            _shapeFactory = shapeFactory;
            _logger = logger;

            New = shapeFactory;
            H = htmlLocalizer;
            S = stringLocalizer;
        }

        public async Task<ActionResult> Index(string name, [ModelBinder(BinderType = typeof(BackgroundJobFilterEngineModelBinder), Name = "q")] QueryFilterResult<BackgroundJobExecution> queryFilterResult, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundJobs))
            {
                return Forbid();
            }

            var options = new BackgroundJobIndexOptions
            {
                BackgroundJobName = name,
            };

            // Populate route values to maintain previous route data when generating page links
            options.FilterResult = queryFilterResult;
            options.FilterResult.MapTo(options);

            // With the options populated we filter the query, allowing the filters to alter the options.
            var users = await _backgroundJobsAdminListQueryService.QueryAsync(options, _updateModelAccessor.ModelUpdater);

            // The search text is provided back to the UI.
            options.SearchText = options.FilterResult.ToString();
            options.OriginalSearchText = options.SearchText;

            // Populate route values to maintain previous route data when generating page links.
            options.RouteValues.TryAdd("q", options.FilterResult.ToString());

            var routeData = new RouteData(options.RouteValues);

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            var count = await users.CountAsync();

            var results = await users
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ListAsync();

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

            var backgroundJobEntries = new List<BackgroundJobViewModelEntry>();

            foreach (var backgroundJobExecution in results)
            {
                backgroundJobEntries.Add(new BackgroundJobViewModelEntry
                {
                    BackgroundJobId = backgroundJobExecution.BackgroundJob.BackgroundJobId,
                    Shape = await _backgroundJobExecutionDisplayManager.BuildDisplayAsync(backgroundJobExecution, updater: _updateModelAccessor.ModelUpdater, displayType: "SummaryAdmin")
                }
                );
            }

            options.UserFilters = new List<SelectListItem>()
            {
                new SelectListItem() { Text = S["All"], Value = nameof(BackgroundJobStatus.Scheduled), Selected = options.Filter == BackgroundJobStatus.Scheduled },
                new SelectListItem() { Text = S["Enabled Users"], Value = nameof(BackgroundJobStatus.Queued), Selected = options.Filter == BackgroundJobStatus.Queued },
                new SelectListItem() { Text = S["Disabled Users"], Value = nameof(BackgroundJobStatus.Executing), Selected = options.Filter == BackgroundJobStatus.Executing }
                //new SelectListItem() { Text = S["Approved"], Value = nameof(UsersFilter.Approved) },
                //new SelectListItem() { Text = S["Email pending"], Value = nameof(UsersFilter.EmailPending) },
                //new SelectListItem() { Text = S["Pending"], Value = nameof(UsersFilter.Pending) }
            };

            options.UserSorts = new List<SelectListItem>()
            {
                new SelectListItem() { Text = S["Name"], Value = nameof(BackgroundJobOrder.Name), Selected = (options.Order == BackgroundJobOrder.Name) },
                new SelectListItem() { Text = S["Email"], Value = nameof(BackgroundJobOrder.Email), Selected = (options.Order == BackgroundJobOrder.Email) },
                //new SelectListItem() { Text = S["Created date"], Value = nameof(UsersOrder.CreatedUtc) },
                //new SelectListItem() { Text = S["Last Login date"], Value = nameof(UsersOrder.LastLoginUtc) }
            };

            options.UsersBulkAction = new List<SelectListItem>()
            {
                new SelectListItem() { Text = S["Approve"], Value = nameof(BackgroundJobBulkAction.Rerun) },
                new SelectListItem() { Text = S["Delete"], Value = nameof(BackgroundJobBulkAction.Delete) }
            };


            // Populate options pager summary values.
            var startIndex = (pagerShape.Page - 1) * (pagerShape.PageSize) + 1;
            options.StartIndex = startIndex;
            options.EndIndex = startIndex + backgroundJobEntries.Count - 1;
            options.BackgrounJobsCount = backgroundJobEntries.Count;
            options.TotalItemCount = pagerShape.TotalItemCount;

            var header = await _backgroundJobOptionsDisplayManager.BuildEditorAsync(options, _updateModelAccessor.ModelUpdater, false);

            var shapeViewModel = await _shapeFactory.CreateAsync<BackgroundJobsIndexViewModel>("BackgroundJobsAdminList", viewModel =>
            {
                viewModel.BackgroundJobs = backgroundJobEntries;
                viewModel.Pager = pagerShape;
                viewModel.Options = options;
                viewModel.Header = header;
            });

            return View(shapeViewModel);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.Filter")]
        public async Task<ActionResult> IndexFilterPOST(BackgroundJobIndexOptions options)
        {
            // When the user has typed something into the search input no further evaluation of the form post is required.
            if (!String.Equals(options.SearchText, options.OriginalSearchText, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Index), new RouteValueDictionary { { "q", options.SearchText } });
            }

            // Evaluate the values provided in the form post and map them to the filter result and route values.
            await _backgroundJobOptionsDisplayManager.UpdateEditorAsync(options, _updateModelAccessor.ModelUpdater, false);

            // The route value must always be added after the editors have updated the models.
            options.RouteValues.TryAdd("q", options.FilterResult.ToString());

            return RedirectToAction(nameof(Index), options.RouteValues);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.BulkAction")]
        public async Task<ActionResult> IndexPOST(BackgroundJobIndexOptions options, IEnumerable<string> itemIds, string name, string returnUrl)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundJobs))
            {
                return Forbid();
            }

            if (itemIds?.Count() > 0)
            {
                var checkedBackgroundJobs = await _session.Query<BackgroundJobExecution, BackgroundJobIndex>().Where(x => x.BackgroundJobId.IsIn(itemIds)).ListAsync();

                switch (options.BulkAction)
                {
                    case BackgroundJobBulkAction.None:
                        break;
                    case BackgroundJobBulkAction.Rerun:
                        foreach (var backgroundJobExecution in checkedBackgroundJobs)
                        {
                        }
                        break;
                    case BackgroundJobBulkAction.Delete:
                        foreach (var backgroundJobExecution in checkedBackgroundJobs)
                        {
                            if (backgroundJobExecution.State.CurrentStatus == BackgroundJobStatus.Queued || backgroundJobExecution.State.CurrentStatus == BackgroundJobStatus.Executing)
                            {
                                await _notifier.ErrorAsync(H["Cannot delete a job that is currently queued or executing."]);
                                continue;
                            }

                            await _backgroundJobStore.DeleteJobAsync(backgroundJobExecution);
                            await _notifier.SuccessAsync(H["Background job {0} successfully deleted.", backgroundJobExecution.BackgroundJob.Name]);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return this.LocalRedirect(returnUrl, true);
            }
            else
            {
                return RedirectToAction(nameof(Index), new { name = name });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id, string name, string returnUrl)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundJobs))
            {
                return Forbid();
            }

            var job = await _backgroundJobStore.GetJobByIdAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            if (job.State.CurrentStatus == BackgroundJobStatus.Queued || job.State.CurrentStatus == BackgroundJobStatus.Executing)
            {
                await _notifier.ErrorAsync(H["Cannot delete a job that is currently queued or executing."]);
                return BadRequest();
            }


            var result = await _backgroundJobStore.DeleteJobAsync(job);

            if (result)
            {
                await _notifier.SuccessAsync(H["Background job deleted successfully."]);
            }
            else
            {
                await _notifier.ErrorAsync(H["Could not delete the background job."]);
            }

            if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return this.LocalRedirect(returnUrl, true);
            }
            else
            {
                return RedirectToAction(nameof(Index), new { name = name });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(string id, string name, string returnUrl)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundJobs))
            {
                return Forbid();
            }

            var result = await _backgroundJobService.TryCancelAsync(id);

            if (result)
            {
                await _notifier.SuccessAsync(H["Background job cancelled successfully."]);
            }
            else
            {
                await _notifier.ErrorAsync(H["Could not cancel the background job."]);
            }

            if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return this.LocalRedirect(returnUrl, true);
            }
            else
            {
                return RedirectToAction(nameof(Index), new { name = name });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteNow(string id, string name, string returnUrl)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundJobs))
            {
                return Forbid();
            }

            var job = await _backgroundJobStore.GetJobByIdAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            if (job.State.CurrentStatus == BackgroundJobStatus.Queued || job.State.CurrentStatus == BackgroundJobStatus.Executing)
            {
                await _notifier.ErrorAsync(H["Cannot execute a job that is currently queued or executing."]);
                return BadRequest();
            }

            var result = await _backgroundJobService.TryRescheduleJobAsync(job.BackgroundJob, _backgroundJobService.Schedule.Now());

            if (result.Success)
            {

                await _notifier.SuccessAsync(H["Background job execution scheduled successfully."]);
            }
            else
            {
                await _notifier.ErrorAsync(H["Error scheduling job execution."]);
            }

            if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return this.LocalRedirect(returnUrl, true);
            }
            else
            {
                return RedirectToAction(nameof(Index), new { name = name });
            }
        }
    }
}

