using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.AdminDashboard.Services;
using OrchardCore.AdminDashboard.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Settings;

namespace OrchardCore.AdminDashboard.Controllers
{
    [Admin]
    public class DashboardController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly ISiteService _siteService;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly INotifier _notifier;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;
        private readonly dynamic New;
        private readonly ILogger _logger;

        public DashboardController(
            IAuthorizationService authorizationService,
            IAdminDashboardService adminDashboardService,
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            ISiteService siteService,
            IUpdateModelAccessor updateModelAccessor,
            IShapeFactory shapeFactory,
            INotifier notifier,
            IStringLocalizer<DashboardController> stringLocalizer,
            IHtmlLocalizer<DashboardController> htmlLocalizer,
            ILogger<DashboardController> logger)
        {
            _authorizationService = authorizationService;
            _adminDashboardService = adminDashboardService;
            _contentManager = contentManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _siteService = siteService;
            _updateModelAccessor = updateModelAccessor;
            New = shapeFactory;
            _notifier = notifier;
            S = stringLocalizer;
            H = htmlLocalizer;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessAdminDashboard))
            {
                return Forbid();
            }

            var widgets = await _adminDashboardService.GetWidgetsAsync(x => x.Published);

            var model = new AdminDashboardViewModel
            {
                Widgets = widgets
            };

            return View(model);
        }
    }
}
