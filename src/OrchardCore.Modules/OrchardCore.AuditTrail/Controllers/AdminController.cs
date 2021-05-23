using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using OrchardCore.AuditTrail.Models;
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
        private readonly IDisplayManager<AuditTrailEvent> _displayManager;

        public AdminController(
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IAuditTrailManager auditTrailManager,
            IUpdateModelAccessor updateModelAccessor,
            IAuthorizationService authorizationService,
            IAuditTrailDisplayManager auditTrailEventDisplayManager,
            IDisplayManager<AuditTrailEvent> displayManager)
        {
            _siteService = siteService;
            _shapeFactory = shapeFactory;
            _auditTrailManager = auditTrailManager;
            _updateModelAccessor = updateModelAccessor;
            _authorizationService = authorizationService;
            _auditTrailEventDisplayManager = auditTrailEventDisplayManager;
            _displayManager = displayManager;
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

            // TODO route data for pager links


            var items = new List<IShape>();

            foreach (var auditTrailEvent in searchResult.Events)
            {
                items.Add(
                    await _displayManager.BuildDisplayAsync(auditTrailEvent, updater: _updateModelAccessor.ModelUpdater, displayType: "SummaryAdmin")
                );
            }

            var shapeViewModel = await _shapeFactory.CreateAsync<AuditTrailListViewModel>("AuditTrailAdminList", viewModel =>
            {
                viewModel.Events = items;
                viewModel.Pager = pagerShape;
                // viewModel.Options = options;
                // viewModel.Header = header;
            });

            return View(shapeViewModel);
        }

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


            var shape = await _displayManager.BuildDisplayAsync(auditTrailEvent, updater: _updateModelAccessor.ModelUpdater, displayType: "DetailAdmin");

            return View(new AuditTrailItemViewModel { Shape = shape});
        }
    }
}
