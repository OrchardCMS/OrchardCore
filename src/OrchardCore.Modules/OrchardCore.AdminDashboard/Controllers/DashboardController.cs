using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Admin;
using OrchardCore.AdminDashboard.Models;
using OrchardCore.AdminDashboard.Services;
using OrchardCore.AdminDashboard.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.ModelBinding;

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
        private readonly YesSql.ISession _session;

        public DashboardController(
            IAuthorizationService authorizationService,
            IAdminDashboardService adminDashboardService,
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            IUpdateModelAccessor updateModelAccessor,
            YesSql.ISession session)
        {
            _authorizationService = authorizationService;
            _adminDashboardService = adminDashboardService;
            _contentManager = contentManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentDefinitionManager = contentDefinitionManager;
            _updateModelAccessor = updateModelAccessor;
            _session = session;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel()
            {
                CanManageDashboard = await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminDashboard),
            };

            if (model.CanManageDashboard || await _authorizationService.AuthorizeAsync(User, Permissions.AccessAdminDashboard))
            {
                var wrappers = new List<DashboardWrapper>();
                var widgets = await _adminDashboardService.GetWidgetsAsync(x => x.Published);
                foreach (var widget in widgets)
                {
                    if (!model.CanManageDashboard && !await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, widget))
                    {
                        continue;
                    }

                    wrappers.Add(new DashboardWrapper
                    {
                        Dashboard = widget,
                        Content = await _contentItemDisplayManager.BuildDisplayAsync(widget, _updateModelAccessor.ModelUpdater, "DetailAdmin")
                    });
                }

                model.Dashboards = wrappers.ToArray();
            }

            return View(model);
        }

        public async Task<IActionResult> Manage()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminDashboard))
            {
                return Forbid();
            }

            // Set Manage Dashboard Feature.
            Request.HttpContext.Features.Set(new DashboardFeature()
            {
                IsManageRequest = true
            });

            var dashboardCreatable = new List<SelectListItem>();

            var widgetContentTypes = _contentDefinitionManager.ListTypeDefinitions()
                    .Where(t => t.StereotypeEquals("DashboardWidget"))
                    .OrderBy(x => x.DisplayName);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            foreach (var ctd in widgetContentTypes)
            {
                if (!await _authorizationService.AuthorizeContentTypeAsync(User, CommonPermissions.EditContent, ctd.Name, userId))
                {
                    continue;
                }

                dashboardCreatable.Add(new SelectListItem(ctd.DisplayName, ctd.Name));
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
                return Unauthorized();
            }

            var contentItemIds = parts.Select(i => i.ContentItemId).ToArray();

            // Load the latest version first if any.
            var latestItems = await _contentManager.GetAsync(contentItemIds, true);

            if (latestItems == null)
            {
                return NotFound();
            }

            var publishedItems = await _contentManager.GetAsync(contentItemIds, false);

            foreach (var contentItem in latestItems)
            {
                var dashboardPart = contentItem.As<DashboardPart>();
                if (dashboardPart == null)
                {
                    return Forbid();
                }

                var partViewModel = parts.FirstOrDefault(m => m.ContentItemId == contentItem.ContentItemId);

                dashboardPart.Position = partViewModel?.Position ?? 0;
                dashboardPart.Width = partViewModel?.Width ?? 1;
                dashboardPart.Height = partViewModel?.Height ?? 1;

                contentItem.Apply(dashboardPart);

                _session.Save(contentItem);

                if (contentItem.IsPublished() == false)
                {
                    var publishedVersion = publishedItems.FirstOrDefault(p => p.ContentItemId == contentItem.ContentItemId);
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
                return Ok();
            }

            return RedirectToAction(nameof(Manage));
        }
    }
}
