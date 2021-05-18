using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Permissions;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.AuditTrail.Controllers
{
    public class AdminController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly IShapeFactory _shapeFactory;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuditTrailDisplayManager _auditTrailEventDisplayManager;

        public AdminController(
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IAuditTrailManager auditTrailManager,
            IUpdateModelAccessor updateModelAccessor,
            IAuthorizationService authorizationService,
            IAuditTrailDisplayManager auditTrailEventDisplayManager)
        {
            _siteService = siteService;
            _shapeFactory = shapeFactory;
            _auditTrailManager = auditTrailManager;
            _updateModelAccessor = updateModelAccessor;
            _authorizationService = authorizationService;
            _auditTrailEventDisplayManager = auditTrailEventDisplayManager;
        }

        public async Task<ActionResult> Index(PagerParameters pagerParameters, AuditTrailOrderBy? orderBy = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, AuditTrailPermissions.ViewAuditTrail))
            {
                return Forbid();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);
            var filters = Filters.From(QueryHelpers.ParseQuery(Request.QueryString.Value), _updateModelAccessor.ModelUpdater);

            var searchResult = await _auditTrailManager.GetEventsAsync(pager.Page, pager.PageSize, filters, orderBy ?? AuditTrailOrderBy.DateDescending);
            if (!_updateModelAccessor.ModelUpdater.ModelState.IsValid)
            {
                searchResult.Events = Enumerable.Empty<AuditTrailEvent>();
            }

            var pagerShape = await _shapeFactory.CreateAsync("Pager", Arguments.From(new
            {
                pager.Page,
                pager.PageSize,
                TotalItemCount = searchResult.TotalCount
            }));

            var categories = _auditTrailManager.DescribeCategories()
                .ToLookup(category => category.Name);

            var eventSummariesViewModel = searchResult.Events
                .Select(@event =>
                {
                    var descriptor = categories[@event.Category]
                        .FirstOrDefault()?.Events
                        .FirstOrDefault(descriptor => descriptor.Name == @event.Name);

                    if (descriptor == null)
                    {
                        descriptor = AuditTrailEventDescriptor.Default(@event);
                    }

                    return new AuditTrailEventSummaryViewModel
                    {
                        Event = @event,
                        LocalizedName = descriptor.LocalizedName,
                        Category = descriptor.LocalizedCategory,
                    };
                })
                .ToArray();

            foreach (var model in eventSummariesViewModel)
            {
                model.SummaryShape = await _auditTrailEventDisplayManager.BuildDisplayEventAsync(model.Event, "SummaryAdmin");
                model.ActionsShape = await _auditTrailEventDisplayManager.BuildDisplayActionsAsync(model.Event, "SummaryAdmin");
            }

            return View(new AuditTrailViewModel
            {
                FiltersShape = await _auditTrailEventDisplayManager.BuildDisplayFiltersAsync(filters),
                OrderBy = orderBy ?? AuditTrailOrderBy.DateDescending,
                Events = eventSummariesViewModel,
                PagerShape = pagerShape
            });
        }

        public async Task<ActionResult> Detail(string auditTrailEventId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, AuditTrailPermissions.ViewAuditTrail))
            {
                return Forbid();
            }

            var @event = await _auditTrailManager.GetEventAsync(auditTrailEventId);
            if (@event == null)
            {
                return NotFound();
            }

            return View(new AuditTrailDetailViewModel
            {
                Event = @event,
                Descriptor = _auditTrailManager.DescribeEvent(@event),
                DetailsShape = await _auditTrailEventDisplayManager.BuildDisplayEventAsync(@event, "Detail")
            });
        }
    }
}
