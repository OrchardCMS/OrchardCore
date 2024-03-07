using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using YesSql.Filters.Query;

namespace OrchardCore.AuditTrail.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuditTrailAdminListQueryService _auditTrailAdminListQueryService;
        private readonly IDisplayManager<AuditTrailEvent> _displayManager;
        private readonly IDisplayManager<AuditTrailIndexOptions> _auditTrailOptionsDisplayManager;

        protected readonly IStringLocalizer S;

        public AdminController(
            IAuditTrailManager auditTrailManager,
            IAuthorizationService authorizationService,
            IAuditTrailAdminListQueryService auditTrailAdminListQueryService,
            IDisplayManager<AuditTrailEvent> displayManager,
            IDisplayManager<AuditTrailIndexOptions> auditTrailOptionsDisplayManager,
            IStringLocalizer<AdminController> stringLocalizer)
        {
            _auditTrailManager = auditTrailManager;
            _authorizationService = authorizationService;
            _auditTrailAdminListQueryService = auditTrailAdminListQueryService;
            _displayManager = displayManager;
            _auditTrailOptionsDisplayManager = auditTrailOptionsDisplayManager;
            S = stringLocalizer;
        }

        [Admin("AuditTrail/{correlationId?}", "AuditTrailIndex")]
        public async Task<ActionResult> Index(
            [FromServices] IOptions<PagerOptions> pagerOptions,
            [FromServices] IShapeFactory shapeFactory,
            [ModelBinder(BinderType = typeof(AuditTrailFilterEngineModelBinder), Name = "q")] QueryFilterResult<AuditTrailEvent> queryFilterResult, PagerParameters pagerParameters, string correlationId = "")
        {
            if (!await _authorizationService.AuthorizeAsync(User, AuditTrailPermissions.ViewAuditTrail))
            {
                return Forbid();
            }

            var options = new AuditTrailIndexOptions
            {
                FilterResult = queryFilterResult
            };

            // This is used by Contents feature for routing so needs to be passed into the options.
            if (!string.IsNullOrEmpty(correlationId))
            {
                options.CorrelationId = correlationId;
                options.CorrelationIdFromRoute = true;
            }

            if (options.CorrelationIdFromRoute)
            {
                // When the correlation id is provided via the route or options a placeholder node is used to apply a filter.
                options.FilterResult.TryAddOrReplace(new CorrelationIdFilterNode(options.CorrelationId));
            }

            var pager = new Pager(pagerParameters, pagerOptions.Value.GetPageSize());

            // With the options populated we filter the query, allowing the filters to alter the options.
            var result = await _auditTrailAdminListQueryService.QueryAsync(pager.Page, pager.PageSize, options);

            // The search text is provided back to the UI.
            options.SearchText = options.FilterResult.ToString();
            options.OriginalSearchText = options.SearchText;

            // Populate route values to maintain previous route data when generating page links.
            options.RouteValues.TryAdd("q", options.FilterResult.ToString());

            var pagerShape = await shapeFactory.PagerAsync(pager, result.TotalCount, options.RouteValues);
            var items = new List<IShape>();

            foreach (var auditTrailEvent in result.Events)
            {
                items.Add(
                    await _displayManager.BuildDisplayAsync(auditTrailEvent, updater: this, displayType: "SummaryAdmin")
                );
            }

            var startIndex = (pager.Page - 1) * pager.PageSize + 1;
            options.StartIndex = startIndex;
            options.EndIndex = startIndex + items.Count - 1;
            options.EventsCount = items.Count;
            options.TotalItemCount = result.TotalCount;

            var header = await _auditTrailOptionsDisplayManager.BuildEditorAsync(options, this, false, string.Empty, string.Empty);

            var shapeViewModel = await shapeFactory.CreateAsync<AuditTrailListViewModel>("AuditTrailAdminList", viewModel =>
            {
                viewModel.Events = items;
                viewModel.Pager = pagerShape;
                viewModel.Options = options;
                viewModel.Header = header;
            });

            return View(shapeViewModel);
        }

        [HttpPost, ActionName(nameof(Index))]
        [FormValueRequired("submit.Filter")]
        public async Task<ActionResult> IndexFilterPOST(AuditTrailIndexOptions options)
        {
            await _auditTrailOptionsDisplayManager.UpdateEditorAsync(options, this, false, string.Empty, string.Empty);
            // When the user has typed something into the search input no further evaluation of the form post is required.
            if (!string.Equals(options.SearchText, options.OriginalSearchText, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Index), new RouteValueDictionary { { "q", options.SearchText } });
            }

            // Evaluate the values provided in the form post and map them to the filter result and route values.
            await _auditTrailOptionsDisplayManager.UpdateEditorAsync(options, this, false, string.Empty, string.Empty);

            // The route value must always be added after the editors have updated the models.
            options.RouteValues.TryAdd("q", options.FilterResult.ToString());

            return RedirectToAction(nameof(Index), options.RouteValues);
        }

        [Admin("AuditTrail/Display/{auditTrailEventId}", "AuditTrailDisplay")]
        public async Task<ActionResult> Display(string auditTrailEventId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, AuditTrailPermissions.ViewAuditTrail))
            {
                return Forbid();
            }

            var auditTrailEvent = await _auditTrailManager.GetEventAsync(auditTrailEventId);
            if (auditTrailEvent == null)
            {
                return NotFound();
            }


            var shape = await _displayManager.BuildDisplayAsync(auditTrailEvent, updater: this, displayType: "DetailAdmin");

            return View(new AuditTrailItemViewModel { Shape = shape });
        }
    }
}
