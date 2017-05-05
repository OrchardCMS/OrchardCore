using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Notify;
using Orchard.Navigation;
using Orchard.Queries.ViewModels;
using Orchard.Settings;

namespace Orchard.Queries.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        private readonly IQueryManager _queryManager;
        private readonly IEnumerable<IQuerySource> _querySources;

        public AdminController(
            IAuthorizationService authorizationService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            INotifier notifier,
            IQueryManager queryManager,
            IEnumerable<IQuerySource> querySources)
        {
            _authorizationService = authorizationService;
            _siteService = siteService;
            _queryManager = queryManager;
            _querySources = querySources;
            New = shapeFactory;
            _notifier = notifier;
            T = stringLocalizer;
        }

        public dynamic New { get; set; }
        public IStringLocalizer T { get; set; }
        public IHtmlLocalizer H { get; set; }

        public async Task<IActionResult> Index(QueryIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            // default options
            if (options == null)
            {
                options = new QueryIndexOptions();
            }

            var queries = await _queryManager.ListQueriesAsync();

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                queries = queries.Where(q => q.Name.IndexOf(options.Search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var results = queries
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ToList();

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = New.Pager(pager).TotalItemCount(queries.Count()).RouteData(routeData);

            var model = new QueriesIndexViewModel
            {
                Queries = results.Select(x =>
                {
                    IShape shape = New.Query_SummaryAdmin(Name: x.Name);
                    shape.Metadata.Alternates.Add("Query_SummaryAdmin__" + x.Name);
                    return new QueryEntry
                    {
                        Query = x,
                        Shape = shape
                    };
                }).ToList(),
                Options = options,
                Pager = pagerShape,
                QuerySourceNames = _querySources.Select(x => x.Name).ToList()
            };

            return View(model);
        }
    }
}
