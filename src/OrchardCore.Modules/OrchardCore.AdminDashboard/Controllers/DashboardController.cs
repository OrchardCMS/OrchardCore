using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.AdminDashboard.Models;
using OrchardCore.AdminDashboard.Services;
using OrchardCore.AdminDashboard.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.AdminDashboard.Controllers
{
    [Admin]
    public class DashboardController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly INotifier _notifier;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;
        private readonly dynamic New;
        private readonly YesSql.ISession _session;
        private readonly ILogger _logger;

        public DashboardController(
            IAuthorizationService authorizationService,
            IAdminDashboardService adminDashboardService,
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            IUpdateModelAccessor updateModelAccessor,
            IShapeFactory shapeFactory,
            INotifier notifier,
            IStringLocalizer<DashboardController> stringLocalizer,
            IHtmlLocalizer<DashboardController> htmlLocalizer,
            YesSql.ISession session,
            ILogger<DashboardController> logger)
        {
            _authorizationService = authorizationService;
            _adminDashboardService = adminDashboardService;
            _contentManager = contentManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentDefinitionManager = contentDefinitionManager;
            _updateModelAccessor = updateModelAccessor;
            New = shapeFactory;
            _notifier = notifier;
            S = stringLocalizer;
            H = htmlLocalizer;
            _session = session;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessAdminDashboard))
            {
                return Forbid();
            }

            var widgets = await _adminDashboardService.GetWidgetsAsync(x => x.Published);
            var wrappers = new List<DashboardWrapper>();
            foreach (var item in widgets)
            {
                wrappers.Add(new DashboardWrapper
                {
                    Dashboard = item,
                    Content = await _contentItemDisplayManager.BuildDisplayAsync(item, _updateModelAccessor.ModelUpdater, "DetailAdmin")
                });
            }

            var model = new AdminDashboardViewModel
            {
                Dashboards = wrappers.ToArray()
            };

            return View(model);
        }

        public async Task<IActionResult> Manage()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminDashboard))
            {
                return Forbid();
            }

            // Set Manage Dashboard Feature
            Request.HttpContext.Features.Set(new DashboardFeature()
            {
                IsManageRequest = true
            });

            var dashboardCreatable = new List<SelectListItem>();

            var widgetContentTypes = _contentDefinitionManager.ListTypeDefinitions()
                    .Where(t =>
                    !string.IsNullOrEmpty(t.GetSettings<ContentTypeSettings>().Stereotype) &&
                    t.GetSettings<ContentTypeSettings>().Stereotype.Contains("DashboardWidget"))
                    .OrderBy(x => x.DisplayName);
            foreach (var ctd in widgetContentTypes)
            {
                var contentItem = await _contentManager.NewAsync(ctd.Name);
                contentItem.Owner = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var authorized = await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem);
                if (authorized)
                {
                    dashboardCreatable.Add(new SelectListItem(ctd.DisplayName, ctd.Name));
                }
            }

            var widgets = await _adminDashboardService.GetWidgetsAsync(x => x.Latest);
            var wrappers = new List<DashboardWrapper>();
            foreach (var item in widgets)
            {
                var wrapper = new DashboardWrapper
                {
                    Dashboard = item,
                    Content = await _contentItemDisplayManager.BuildDisplayAsync(item, _updateModelAccessor.ModelUpdater, "DetailAdmin")
                };
                wrappers.Add(wrapper);
            }

            var model = new AdminDashboardViewModel
            {
                Dashboards = wrappers.ToArray(),
                Creatable = dashboardCreatable
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Update([FromForm] DashboardPartViewModel[] parts)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminDashboard))
            {
                return StatusCode(401);
            }

            var contentItemIds = parts.Select(i => i.ContentItemId).ToArray();

            // Load the latest version first if any
            var latestItems = await _contentManager.GetAsync(contentItemIds, true);

            var publishedItems = await _contentManager.GetAsync(contentItemIds, false);

            if (latestItems == null)
            {
                return StatusCode(404);
            }

            foreach (var contentItem in latestItems)
            {
                var dashboardPart = contentItem.As<DashboardPart>();
                if (dashboardPart == null)
                {
                    return StatusCode(403);
                }

                var partViewModel = parts.Where(m => m.ContentItemId == contentItem.ContentItemId).FirstOrDefault();

                dashboardPart.Position = partViewModel?.Position ?? 0;
                dashboardPart.Width = partViewModel?.Width ?? 1;
                dashboardPart.Height = partViewModel?.Height ?? 1;

                contentItem.Apply(dashboardPart);

                _session.Save(contentItem);

                if (contentItem.IsPublished() == false)
                {
                    var publishedVersion = publishedItems.Where(p => p.ContentItemId == contentItem.ContentItemId).FirstOrDefault();
                    var publishedMetaData = publishedVersion?.As<DashboardPart>();
                    if (publishedVersion != null && publishedMetaData != null)
                    {
                        publishedMetaData.Position = partViewModel.Position;
                        publishedMetaData.Width = partViewModel.Width;
                        publishedMetaData.Height = partViewModel.Height;
                        publishedVersion.Apply(publishedMetaData);
                        _session.Save(publishedVersion);
                    }
                }

            }

            if (Request.Headers != null && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return StatusCode(200);
            }
            else
            {
                return RedirectToAction(nameof(Manage));
            }

        }
    }
}
