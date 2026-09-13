using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using Microsoft.Extensions.Localization;
using OrchardCore.Sitemaps.Models;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement;
using OrchardCore.Sitemaps.Cache;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Controllers;

[Admin("SitemapsCache/{action}/{cacheFileName?}", "SitemapsCache{action}")]
public sealed class SitemapCacheController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ISitemapCacheProvider _sitemapCacheProvider;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public SitemapCacheController(
        IAuthorizationService authorizationService,
        ISitemapCacheProvider sitemapCacheProvider,
        INotifier notifier,
        IHtmlLocalizer<SitemapCacheController> htmlLocalizer,
        IStringLocalizer<SitemapCacheController> stringLocalizer
        )
    {
        _authorizationService = authorizationService;
        _sitemapCacheProvider = sitemapCacheProvider;
        _notifier = notifier;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    public async Task<IActionResult> List(
        [FromServices] IShapeFactory shapeFactory,
        [FromServices] IDisplayManager<SitemapCacheEntry> displayManager,
        [FromServices] IUpdateModelAccessor updateModelAccessor,
        [FromServices] IAdminListService adminListService)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SitemapsPermissions.ManageSitemaps))
        {
            return Forbid();
        }

        var model = new ListSitemapCacheViewModel
        {
            CachedFileNames = (await _sitemapCacheProvider.ListAsync()).ToArray(),
        };

        var rows = new List<object>(model.CachedFileNames.Length);

        foreach (var fileName in model.CachedFileNames)
        {
            var shape = await displayManager.BuildDisplayAsync(new SitemapCacheEntry { FileName = fileName }, updateModelAccessor.ModelUpdater, OrchardCoreConstants.DisplayType.SummaryAdmin);

            // The rows carry the attributes used by the client-side search of the list-management script.
            if (shape is Shape rowShape)
            {
                rowShape.Classes.Add("item");
                rowShape.Attributes["data-filter-value"] = fileName.ToLowerInvariant();
            }

            rows.Add(shape);
        }

        var toolbar = await shapeFactory.CreateAsync("AdminListToolbar", Arguments.From(new
        {
            ItemsCount = rows.Count,
            ShowSelectAll = false,
        }));

        // The AdminList shape renders the cached files with the configured layout (List, Table, ...).
        model.List = await shapeFactory.CreateAsync(AdminListConstants.ShapeType, Arguments.From(new
        {
            Name = SitemapCacheAdminList.Name,
            Layout = await adminListService.GetLayoutAsync(SitemapCacheAdminList.Name, cancellationToken: HttpContext.RequestAborted),
            Columns = await adminListService.GetColumnsAsync(SitemapCacheAdminList.Name, SitemapCacheAdminList.GetDefaultColumns(S), cancellationToken: HttpContext.RequestAborted),
            Rows = rows,
            Toolbar = toolbar,
            ItemCssClass = "list-group-item",
            EmptyMessage = H["<strong>Nothing here!</strong> There are no sitemaps cached for the moment."],
        }));

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> PurgeAll()
    {
        if (!await _authorizationService.AuthorizeAsync(User, SitemapsPermissions.ManageSitemaps))
        {
            return Forbid();
        }

        var hasErrors = await _sitemapCacheProvider.PurgeAllAsync();
        if (hasErrors)
        {
            await _notifier.ErrorAsync(H["Sitemap cache purged, with errors."]);
        }
        else
        {
            await _notifier.InformationAsync(H["Sitemap cache purged."]);
        }

        return RedirectToAction(nameof(List));
    }

    [HttpPost]
    public async Task<IActionResult> Purge(string cacheFileName)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SitemapsPermissions.ManageSitemaps))
        {
            return Forbid();
        }

        var failed = await _sitemapCacheProvider.PurgeAsync(cacheFileName);
        if (failed)
        {
            await _notifier.ErrorAsync(H["Error purging sitemap cache item."]);
        }
        else
        {
            await _notifier.InformationAsync(H["Sitemap cache item purged."]);
        }

        return RedirectToAction(nameof(List));
    }
}
