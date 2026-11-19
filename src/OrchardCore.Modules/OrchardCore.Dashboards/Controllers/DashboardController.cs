using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents;
using OrchardCore.Dashboards.Models;
using OrchardCore.Dashboards.Services;
using OrchardCore.Dashboards.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Dashboards.Controllers;

[Admin]
public sealed class DashboardController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IDashboardManager _dashboardManager;
    private readonly IContentManager _contentManager;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizerFactory _stringLocalizerFactory;
    private readonly YesSql.ISession _session;

    public DashboardController(
        IAuthorizationService authorizationService,
        IDashboardManager dashboardManager,
        IContentManager contentManager,
        IContentItemDisplayManager contentItemDisplayManager,
        IContentDefinitionManager contentDefinitionManager,
        IUpdateModelAccessor updateModelAccessor,
        IStringLocalizerFactory stringLocalizerFactory,
        YesSql.ISession session)
    {
        _authorizationService = authorizationService;
        _dashboardManager = dashboardManager;
        _contentManager = contentManager;
        _contentItemDisplayManager = contentItemDisplayManager;
        _contentDefinitionManager = contentDefinitionManager;
        _updateModelAccessor = updateModelAccessor;
        _stringLocalizerFactory = stringLocalizerFactory;
        _session = session;
    }

    [Admin("dashboards/{type}", "Dashboards")]
    public async Task<IActionResult> Index(string type)
    {
        var dashboardType = _dashboardManager.GetType(type);

        if (dashboardType is null)
        {
            return NotFound();
        }

        HttpContext.Features.Set(new DashboardFeature
        {
            DashboardType = dashboardType,
        });

        var model = new DashboardViewModel
        {
            DashboardType = dashboardType,
            Title = _stringLocalizerFactory.Localize(dashboardType.DisplayName),
            CanManageDashboard = await _dashboardManager.CanManageAsync(User, dashboardType),
        };

        model.CanViewDashboard = model.CanManageDashboard || await _dashboardManager.CanViewAsync(User, dashboardType);

        if (model.CanViewDashboard)
        {
            var wrappers = new List<DashboardWrapper>();
            var widgetContentTypes = await GetWidgetContentTypesAsync();

            // The widgets of a dashboard of their own are shown to their owner, who manages them.
            var checkViewPermission = !model.CanManageDashboard && dashboardType.Scope == DashboardScope.Site;

            foreach (var widget in await _dashboardManager.GetWidgetsAsync(User, dashboardType, latest: false))
            {
                if (!widgetContentTypes.ContainsKey(widget.ContentType))
                {
                    continue;
                }

                if (checkViewPermission && !await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, widget))
                {
                    continue;
                }

                wrappers.Add(new DashboardWrapper
                {
                    Dashboard = widget,
                    Content = await _contentItemDisplayManager.BuildDisplayAsync(widget, _updateModelAccessor.ModelUpdater, OrchardCoreConstants.DisplayType.DetailAdmin),
                });
            }

            model.Dashboards = wrappers.ToArray();
        }

        return View(model);
    }

    [Admin("dashboards/{type}/manage", "DashboardsManage")]
    public async Task<IActionResult> Manage(string type)
    {
        var dashboardType = _dashboardManager.GetType(type);

        if (dashboardType is null)
        {
            return NotFound();
        }

        if (!await _dashboardManager.CanManageAsync(User, dashboardType))
        {
            return Forbid();
        }

        HttpContext.Features.Set(new DashboardFeature
        {
            DashboardType = dashboardType,
            IsManageRequest = true,
        });

        var dashboardCreatable = new List<SelectListItem>();
        var widgetContentTypes = await GetWidgetContentTypesAsync();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        foreach (var contentTypeDefinition in widgetContentTypes.Values.OrderBy(x => x.DisplayName))
        {
            if (!await _authorizationService.AuthorizeContentTypeAsync(User, CommonPermissions.EditContent, contentTypeDefinition.Name, userId))
            {
                continue;
            }

            dashboardCreatable.Add(new SelectListItem(contentTypeDefinition.DisplayName, contentTypeDefinition.Name));
        }

        var wrappers = new List<DashboardWrapper>();

        foreach (var widget in await _dashboardManager.GetWidgetsAsync(User, dashboardType, latest: true))
        {
            if (!widgetContentTypes.ContainsKey(widget.ContentType) ||
                !await _authorizationService.AuthorizeContentTypeAsync(User, CommonPermissions.EditContent, widget.ContentType, userId))
            {
                continue;
            }

            wrappers.Add(new DashboardWrapper
            {
                Dashboard = widget,
                Content = await _contentItemDisplayManager.BuildDisplayAsync(widget, _updateModelAccessor.ModelUpdater, OrchardCoreConstants.DisplayType.DetailAdmin),
            });
        }

        var model = new DashboardViewModel
        {
            DashboardType = dashboardType,
            Title = _stringLocalizerFactory.Localize(dashboardType.DisplayName),
            CanViewDashboard = true,
            CanManageDashboard = true,
            Dashboards = wrappers.ToArray(),
            Creatable = dashboardCreatable,
        };

        return View(model);
    }

    [HttpPost]
    [Admin("dashboards/{type}/update", "DashboardsUpdate")]
    public async Task<IActionResult> Update(string type, [FromForm] DashboardPartViewModel[] parts)
    {
        var dashboardType = _dashboardManager.GetType(type);

        if (dashboardType is null)
        {
            return NotFound();
        }

        if (!await _dashboardManager.CanManageAsync(User, dashboardType))
        {
            return Forbid();
        }

        var contentItemIds = parts.Select(i => i.ContentItemId).ToArray();

        // Load the latest version first if any.
        var latestItems = await _contentManager.GetAsync(contentItemIds, VersionOptions.Latest);

        if (latestItems == null)
        {
            return NotFound();
        }

        var publishedItems = await _contentManager.GetAsync(contentItemIds, VersionOptions.Published);

        foreach (var contentItem in latestItems)
        {
            // Only the widgets of the dashboard the user manages can be arranged from it.
            if (!contentItem.TryGet<DashboardPart>(out var dashboardPart) ||
                !_dashboardManager.IsOnDashboard(User, dashboardType, contentItem))
            {
                return Forbid();
            }

            var partViewModel = parts.FirstOrDefault(m => m.ContentItemId == contentItem.ContentItemId);

            dashboardPart.Position = partViewModel?.Position ?? 0;
            dashboardPart.Width = partViewModel?.Width ?? 1;
            dashboardPart.Height = partViewModel?.Height ?? 1;

            contentItem.Apply(dashboardPart);

            await _session.SaveAsync(contentItem);

            if (contentItem.IsPublished() == false)
            {
                var publishedVersion = publishedItems.FirstOrDefault(p => p.ContentItemId == contentItem.ContentItemId);
                if (publishedVersion?.TryGet<DashboardPart>(out var publishedMetaData) == true)
                {
                    publishedMetaData.Position = dashboardPart.Position;
                    publishedMetaData.Width = dashboardPart.Width;
                    publishedMetaData.Height = dashboardPart.Height;
                    publishedVersion.Apply(publishedMetaData);
                    await _session.SaveAsync(publishedVersion);
                }
            }
        }

        if (Request.Headers != null && Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return Ok();
        }

        return RedirectToAction(nameof(Manage), new { type = dashboardType.Name });
    }

    private async Task<Dictionary<string, ContentTypeDefinition>> GetWidgetContentTypesAsync()
        => (await _contentDefinitionManager.ListTypeDefinitionsAsync())
        .Where(t => t.StereotypeEquals(DashboardConstants.WidgetStereotype))
        .ToDictionary(ctd => ctd.Name, ctd => ctd);
}
