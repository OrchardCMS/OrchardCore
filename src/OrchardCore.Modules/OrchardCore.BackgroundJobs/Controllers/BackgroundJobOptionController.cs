using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.BackgroundJobs.Services;
using OrchardCore.BackgroundJobs.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Settings;

namespace OrchardCore.BackgroundJobs.Controllers
{
    [Admin]
    public class BackgroundJobOptionController : Controller
    {
        private readonly BackgroundJobOptions _backgroundJobOptions;
        private readonly ISiteService _siteService;
        private readonly IAuthorizationService _authorizationService;

        private readonly dynamic New;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;

        public BackgroundJobOptionController(
            IOptions<BackgroundJobOptions> options,
            ISiteService siteService,
            IAuthorizationService authorizationService,
            IShapeFactory shapeFactory,
            IStringLocalizer<BackgroundJobOptionController> s,
            IHtmlLocalizer<BackgroundJobOptionController> h
            )
        {
            _backgroundJobOptions = options.Value;
            _siteService = siteService;
            _authorizationService = authorizationService;

            New = shapeFactory;
            S = s;
            H = h;
        }

        // TODO Remove
        public async Task<ActionResult> Run()
        {
            var scheduler = new ScheduleJobsBackgroundTask();
            await scheduler.DoWorkAsync(HttpContext.RequestServices, default);
            return Ok();
        }

        public async Task<IActionResult> Index(BackgroundJobTypeIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundJobs))
            {
                return Forbid();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            if (options == null)
            {
                options = new BackgroundJobTypeIndexOptions();
            }

            var jobs = _backgroundJobOptions.BackgroundJobs.AsEnumerable();


            switch (options.Filter)
            {
                case BackgroundJobTypeFilter.All:
                default:
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                jobs = jobs.Where(x => x.Key.Contains(options.Search, StringComparison.OrdinalIgnoreCase));
            }

            switch (options.Order)
            {
                case BackgroundJobTypeOrder.Name:
                    jobs = jobs.OrderBy(u => u.Key);
                    break;
            }

            var count = jobs.Count();

            var jobTypes = jobs
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                ;

            // Maintain previous route data when generating page links.
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);
            var model = new BackgroundJobOptionIndexViewModel
            {
                JobOptions = jobTypes
                    .Select(x => new BackgroundJobOptionEntry
                    {
                        JobOption = x.Value,
                        //Id = x.Id,
                        //WorkflowCount = workflowGroups.ContainsKey(x.WorkflowTypeId) ? workflowGroups[x.WorkflowTypeId].Count() : 0,
                        Name = x.Key
                    })
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };


            return View(model);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.Filter")]
        public ActionResult IndexFilterPOST(BackgroundJobOptionIndexViewModel model)
        {
            return RedirectToAction(nameof(Index),
                new RouteValueDictionary
                {
                    { "Options.Search", model.Options.Search }
                });
        }

    }
}
